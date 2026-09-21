using System;
using System.Collections.Generic;
using MajoServerModpack.Core.Input;
using MajoServerModpack.Core.Logging;
using MajoServerModpack.Core.Modules;
using MajoServerModpack.Core.Patching;
using MajoServerModpack.Core.Runtime;

internal static class Program
{
    private static int _failed;

    private static int Main()
    {
        Run("Module duplicate IDs", TestDuplicateModuleIds);
        Run("Module missing dependency", TestMissingDependency);
        Run("Module cycle", TestDependencyCycle);
        Run("Module lifecycle", TestLifecycle);
        Run("Module rejects reinitialization", TestModuleReinitialization);
        Run("Module failure isolation", TestFailureIsolation);
        Run("Input duplicate action", TestDuplicateInputAction);
        Run("Input conflict same context", TestInputConflictSameContext);
        Run("Input different contexts", TestInputDifferentContexts);
        Run("Input global context", TestInputGlobalContext);
        Run("Input shared binding", TestInputSharedBinding);
        Run("Patch owner ambiguity", TestPatchOwnerAmbiguity);
        Run("Patch surface normalization", TestPatchSurfaceNormalization);
        Run("Patch consumers", TestPatchConsumers);
        Run("Runtime metadata", TestRuntimeMetadata);
        Run("Runtime rejects bootstrap after shutdown", TestBootstrapAfterShutdown);
        _failed += NetworkingTests.RunAll();

        Console.WriteLine(_failed == 0
            ? "Majo core tests: PASS"
            : "Majo core tests: FAIL (" + _failed + ")");

        return _failed == 0 ? 0 : 1;
    }

    private static void Run(string name, Action test)
    {
        try
        {
            test();
            Console.WriteLine("[PASS] " + name);
        }
        catch (Exception exception)
        {
            _failed++;
            Console.WriteLine("[FAIL] " + name + ": " + exception.Message);
        }
    }

    private static void TestDuplicateModuleIds()
    {
        var registry = Registry();
        registry.Register(Module("a"));

        AssertThrows<InvalidOperationException>(() => registry.Register(Module("a")));
    }

    private static void TestMissingDependency()
    {
        var registry = Registry();
        registry.Register(Module("a", dependencies: new[] { "missing" }));

        Assert(registry.ValidateDependencies().Count == 1, "missing dependency must be reported");
    }

    private static void TestDependencyCycle()
    {
        var registry = Registry();
        registry.Register(Module("a", dependencies: new[] { "b" }));
        registry.Register(Module("b", dependencies: new[] { "a" }));

        Assert(registry.ValidateDependencies().Count > 0, "cycle must be reported");
    }

    private static void TestLifecycle()
    {
        var events = new List<string>();
        var registry = Registry();
        registry.Register(Module("base", events));
        registry.Register(Module("dependent", events, new[] { "base" }));

        registry.InitializeAll();
        registry.StartAll();
        registry.StopAll();

        AssertSequence(events,
            "base:init", "dependent:init",
            "base:start", "dependent:start",
            "dependent:stop", "base:stop");
    }

    private static void TestModuleReinitialization()
    {
        var registry = Registry();
        registry.Register(Module("a"));

        registry.InitializeAll();
        registry.StartAll();

        AssertThrows<InvalidOperationException>(() => registry.InitializeAll());
        registry.StopAll();

        Assert(State(registry.Snapshot(), "a") == ModuleLifecycleState.Stopped,
            "started module must still be stoppable after rejected reinitialization");
    }

    private static void TestFailureIsolation()
    {
        var events = new List<string>();
        var registry = Registry();
        registry.Register(Module("broken", events, throwOnStart: true));
        registry.Register(Module("dependent", events, new[] { "broken" }));
        registry.Register(Module("independent", events));

        registry.InitializeAll();
        registry.StartAll();

        var snapshots = registry.Snapshot();
        Assert(State(snapshots, "broken") == ModuleLifecycleState.Failed, "broken module must fail");
        Assert(State(snapshots, "dependent") == ModuleLifecycleState.Failed, "dependent module must fail safely");
        Assert(State(snapshots, "independent") == ModuleLifecycleState.Started, "independent module must still start");
    }

    private static void TestDuplicateInputAction()
    {
        var registry = new InputRegistry();
        registry.Register(Action("a", "Gameplay", "F8"));

        AssertThrows<InvalidOperationException>(() => registry.Register(Action("a", "Gameplay", "F9")));
    }

    private static void TestInputConflictSameContext()
    {
        var registry = new InputRegistry();
        registry.Register(Action("a", "Gameplay", "F8"));
        var conflicts = registry.Register(Action("b", "Gameplay", "F8"));

        Assert(conflicts.Count == 1, "same context must conflict");
        var result = registry.TrySetBinding("b", new InputBinding(InputDevice.Keyboard, "F8"));
        Assert(!result.Applied, "conflicting rebinding must not apply");
    }

    private static void TestInputDifferentContexts()
    {
        var registry = new InputRegistry();
        registry.Register(Action("a", "Gameplay", "F8"));
        var conflicts = registry.Register(Action("b", "Map", "F8"));

        Assert(conflicts.Count == 0, "different contexts may reuse a binding");
    }

    private static void TestInputGlobalContext()
    {
        var registry = new InputRegistry();
        registry.Register(Action("a", "Global", "F8"));
        var conflicts = registry.Register(Action("b", "Map", "F8"));

        Assert(conflicts.Count == 1, "Global context must conflict with another context");
    }

    private static void TestInputSharedBinding()
    {
        var registry = new InputRegistry();
        registry.Register(Action("a", "Gameplay", "F8", allowShared: true));
        var conflicts = registry.Register(Action("b", "Gameplay", "F8", allowShared: true));

        Assert(conflicts.Count == 0, "explicitly shared bindings must be accepted");
    }

    private static void TestPatchOwnerAmbiguity()
    {
        var patches = new PatchCoordinator();
        patches.RegisterOwner("ZNet.Awake", "core.runtime");
        patches.RegisterOwner("ZNet.Awake", "core.runtime");

        AssertThrows<InvalidOperationException>(() => patches.RegisterOwner("ZNet.Awake", "other"));
    }

    private static void TestPatchSurfaceNormalization()
    {
        var patches = new PatchCoordinator();
        patches.RegisterOwner("ZNet.Awake", "core.runtime");
        patches.RegisterOwner(" ZNet.Awake ", " core.runtime ");

        Assert(patches.Count == 1, "surface whitespace must not create a second registry entry");
        AssertThrows<InvalidOperationException>(
            () => patches.RegisterOwner(" ZNet.Awake ", "other"));
    }

    private static void TestPatchConsumers()
    {
        var patches = new PatchCoordinator();
        patches.RegisterConsumer("ZNet.Awake", "diagnostics");
        patches.RegisterConsumer("ZNet.Awake", "diagnostics");
        patches.RegisterOwner("ZNet.Awake", "core.runtime");

        var snapshot = patches.Snapshot();
        Assert(snapshot.Count == 1, "surface must exist");
        Assert(snapshot[0].Consumers.Count == 1, "consumer registration must be idempotent");
    }

    private static void TestRuntimeMetadata()
    {
        var metadata = CreateRuntimeMetadata();
        Assert(metadata.MajoVersion == "0.0.2", "runtime must be 0.0.2");
        Assert(metadata.ProtocolVersion == 1, "protocol v1 must be active");
        Assert(metadata.ConfigSchema == 0, "config schema must remain unimplemented");
        Assert(metadata.DataSchema == 0, "data schema must remain unimplemented");
    }

    private static void TestBootstrapAfterShutdown()
    {
        var runtime = new MajoRuntime(
            new TestLogger(),
            new TestExecutionContextProvider(),
            CreateRuntimeMetadata());

        runtime.Shutdown();

        AssertThrows<InvalidOperationException>(() => runtime.Bootstrap());
    }

    private static RuntimeMetadata CreateRuntimeMetadata()
    {
        return new RuntimeMetadata(
            MajoVersions.MajoVersion,
            MajoVersions.ProtocolVersion,
            MajoVersions.ConfigSchema,
            MajoVersions.DataSchema,
            "1.0.15",
            "5.4.23.5",
            "2.30.1.0");
    }

    private static ModuleRegistry Registry()
    {
        return new ModuleRegistry(new TestLogger());
    }

    private static TestModule Module(
        string id,
        IList<string> events = null,
        IEnumerable<string> dependencies = null,
        bool throwOnStart = false)
    {
        return new TestModule(id, events, dependencies, throwOnStart);
    }

    private static InputActionDescriptor Action(
        string id,
        string context,
        string key,
        bool allowShared = false)
    {
        return new InputActionDescriptor(
            id,
            "test.module",
            context,
            new InputBinding(InputDevice.Keyboard, key),
            allowShared);
    }

    private static ModuleLifecycleState State(IReadOnlyList<ModuleSnapshot> modules, string id)
    {
        foreach (var module in modules)
        {
            if (module.Descriptor.ModuleId == id)
            {
                return module.State;
            }
        }

        throw new InvalidOperationException("Missing module snapshot: " + id);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertThrows<T>(Action action) where T : Exception
    {
        try
        {
            action();
        }
        catch (T)
        {
            return;
        }

        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }

    private static void AssertSequence(IList<string> actual, params string[] expected)
    {
        Assert(actual.Count == expected.Length,
            "sequence length mismatch: actual=" + actual.Count + " expected=" + expected.Length);

        for (var index = 0; index < expected.Length; index++)
        {
            Assert(actual[index] == expected[index],
                "sequence mismatch at " + index + ": " + actual[index] + " != " + expected[index]);
        }
    }

    private sealed class TestExecutionContextProvider : IExecutionContextProvider
    {
        public ExecutionContextSnapshot Detect()
        {
            return new ExecutionContextSnapshot(ExecutionContextKind.MenuOrPreWorld, "test");
        }
    }

    private sealed class TestLogger : IMajoLogger
    {
        public void Debug(string category, string message) { }
        public void Info(string category, string message) { }
        public void Warning(string category, string message) { }
        public void Error(string category, string message, Exception exception = null) { }
    }

    private sealed class TestModule : IMajoModule
    {
        private readonly IList<string> _events;
        private readonly bool _throwOnStart;

        public TestModule(
            string id,
            IList<string> events,
            IEnumerable<string> dependencies,
            bool throwOnStart)
        {
            _events = events;
            _throwOnStart = throwOnStart;
            Descriptor = new ModuleDescriptor(
                id,
                id,
                "0.0.1",
                ModuleExecutionSide.ClientAndServer,
                dependencies);
        }

        public ModuleDescriptor Descriptor { get; }

        public void Initialize(ModuleContext context)
        {
            _events?.Add(Descriptor.ModuleId + ":init");
        }

        public void Start()
        {
            _events?.Add(Descriptor.ModuleId + ":start");
            if (_throwOnStart)
            {
                throw new InvalidOperationException("planned test failure");
            }
        }

        public void Stop()
        {
            _events?.Add(Descriptor.ModuleId + ":stop");
        }
    }
}
