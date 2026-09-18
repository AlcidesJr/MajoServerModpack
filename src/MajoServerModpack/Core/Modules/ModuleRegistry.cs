using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MajoServerModpack.Core.Logging;

namespace MajoServerModpack.Core.Modules
{
    public sealed class ModuleRegistry
    {
        private sealed class Entry
        {
            public Entry(IMajoModule module)
            {
                Module = module;
                State = ModuleLifecycleState.Registered;
            }

            public IMajoModule Module { get; }
            public ModuleLifecycleState State { get; set; }
            public string Failure { get; set; }
        }

        private readonly Dictionary<string, Entry> _entries =
            new Dictionary<string, Entry>(StringComparer.Ordinal);
        private readonly List<string> _registrationOrder = new List<string>();
        private readonly IMajoLogger _logger;

        public ModuleRegistry(IMajoLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public int Count => _entries.Count;

        public void Register(IMajoModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            var descriptor = module.Descriptor ?? throw new ArgumentException("Module descriptor is required.", nameof(module));
            if (_entries.ContainsKey(descriptor.ModuleId))
            {
                throw new InvalidOperationException("Duplicate ModuleId: " + descriptor.ModuleId);
            }

            foreach (var dependency in descriptor.Dependencies)
            {
                if (string.Equals(dependency, descriptor.ModuleId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Module cannot depend on itself: " + descriptor.ModuleId);
                }
            }

            _entries.Add(descriptor.ModuleId, new Entry(module));
            _registrationOrder.Add(descriptor.ModuleId);
        }

        public IReadOnlyList<string> ValidateDependencies()
        {
            var errors = new List<string>();

            foreach (var moduleId in _registrationOrder)
            {
                var descriptor = _entries[moduleId].Module.Descriptor;
                foreach (var dependency in descriptor.Dependencies)
                {
                    if (!_entries.ContainsKey(dependency))
                    {
                        errors.Add(moduleId + " requires missing module " + dependency);
                    }
                }
            }

            try
            {
                BuildTopologicalOrder();
            }
            catch (InvalidOperationException exception)
            {
                errors.Add(exception.Message);
            }

            return new ReadOnlyCollection<string>(errors);
        }

        public void InitializeAll()
        {
            var order = RequireValidOrder();
            var context = new ModuleContext(_logger);

            foreach (var moduleId in order)
            {
                var entry = _entries[moduleId];
                if (HasFailedDependency(entry.Module.Descriptor, ModuleLifecycleState.Initialized))
                {
                    Fail(entry, "Dependency failed before initialization.", null);
                    continue;
                }

                try
                {
                    entry.Module.Initialize(context);
                    entry.State = ModuleLifecycleState.Initialized;
                }
                catch (Exception exception)
                {
                    Fail(entry, "Initialize failed.", exception);
                }
            }
        }

        public void StartAll()
        {
            var order = RequireValidOrder();

            foreach (var moduleId in order)
            {
                var entry = _entries[moduleId];
                if (entry.State != ModuleLifecycleState.Initialized)
                {
                    continue;
                }

                if (HasFailedDependency(entry.Module.Descriptor, ModuleLifecycleState.Started))
                {
                    Fail(entry, "Dependency did not reach Started.", null);
                    continue;
                }

                try
                {
                    entry.Module.Start();
                    entry.State = ModuleLifecycleState.Started;
                }
                catch (Exception exception)
                {
                    Fail(entry, "Start failed.", exception);
                }
            }
        }

        public void StopAll()
        {
            var order = RequireValidOrder();

            for (var index = order.Count - 1; index >= 0; index--)
            {
                var entry = _entries[order[index]];
                if (entry.State != ModuleLifecycleState.Started)
                {
                    continue;
                }

                try
                {
                    entry.Module.Stop();
                    entry.State = ModuleLifecycleState.Stopped;
                }
                catch (Exception exception)
                {
                    Fail(entry, "Stop failed.", exception);
                }
            }
        }

        public IReadOnlyList<ModuleSnapshot> Snapshot()
        {
            var result = new List<ModuleSnapshot>(_registrationOrder.Count);
            foreach (var moduleId in _registrationOrder)
            {
                var entry = _entries[moduleId];
                result.Add(new ModuleSnapshot(entry.Module.Descriptor, entry.State, entry.Failure));
            }

            return new ReadOnlyCollection<ModuleSnapshot>(result);
        }

        private IReadOnlyList<string> RequireValidOrder()
        {
            var errors = ValidateDependencies();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException("Invalid module graph: " + string.Join("; ", errors));
            }

            return BuildTopologicalOrder();
        }

        private bool HasFailedDependency(ModuleDescriptor descriptor, ModuleLifecycleState requiredState)
        {
            foreach (var dependencyId in descriptor.Dependencies)
            {
                var state = _entries[dependencyId].State;
                if (state == ModuleLifecycleState.Failed || state != requiredState)
                {
                    return true;
                }
            }

            return false;
        }

        private void Fail(Entry entry, string message, Exception exception)
        {
            entry.State = ModuleLifecycleState.Failed;
            entry.Failure = exception == null ? message : message + " " + exception.Message;
            _logger.Error("Modules/" + entry.Module.Descriptor.ModuleId, message, exception);
        }

        private IReadOnlyList<string> BuildTopologicalOrder()
        {
            var order = new List<string>(_entries.Count);
            var marks = new Dictionary<string, byte>(StringComparer.Ordinal);

            foreach (var moduleId in _registrationOrder)
            {
                Visit(moduleId, marks, order);
            }

            return new ReadOnlyCollection<string>(order);
        }

        private void Visit(string moduleId, IDictionary<string, byte> marks, IList<string> order)
        {
            byte mark;
            if (marks.TryGetValue(moduleId, out mark))
            {
                if (mark == 1)
                {
                    throw new InvalidOperationException("Dependency cycle detected at " + moduleId);
                }

                if (mark == 2)
                {
                    return;
                }
            }

            marks[moduleId] = 1;
            var descriptor = _entries[moduleId].Module.Descriptor;

            foreach (var dependency in descriptor.Dependencies)
            {
                if (_entries.ContainsKey(dependency))
                {
                    Visit(dependency, marks, order);
                }
            }

            marks[moduleId] = 2;
            order.Add(moduleId);
        }
    }
}
