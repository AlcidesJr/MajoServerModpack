using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MajoServerModpack.Core.Logging;

namespace MajoServerModpack.Core.Modules
{
    public enum ModuleExecutionSide
    {
        ClientOnly,
        ServerOnly,
        ClientAndServer,
        OptionalClient
    }

    public enum ModuleRiskLevel
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum ModuleLifecycleState
    {
        Registered,
        Initialized,
        Started,
        Stopped,
        Failed
    }

    public sealed class ModuleDescriptor
    {
        public ModuleDescriptor(
            string moduleId,
            string name,
            string version,
            ModuleExecutionSide executionSide,
            IEnumerable<string> dependencies = null,
            IEnumerable<string> capabilities = null,
            ModuleRiskLevel riskLevel = ModuleRiskLevel.Low)
        {
            if (string.IsNullOrWhiteSpace(moduleId))
            {
                throw new ArgumentException("ModuleId is required.", nameof(moduleId));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Module name is required.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentException("Module version is required.", nameof(version));
            }

            ModuleId = moduleId.Trim();
            Name = name.Trim();
            Version = version.Trim();
            ExecutionSide = executionSide;
            Dependencies = Copy(dependencies);
            Capabilities = Copy(capabilities);
            RiskLevel = riskLevel;
        }

        public string ModuleId { get; }
        public string Name { get; }
        public string Version { get; }
        public ModuleExecutionSide ExecutionSide { get; }
        public IReadOnlyList<string> Dependencies { get; }
        public IReadOnlyList<string> Capabilities { get; }
        public ModuleRiskLevel RiskLevel { get; }

        private static IReadOnlyList<string> Copy(IEnumerable<string> values)
        {
            var list = new List<string>();
            if (values != null)
            {
                foreach (var value in values)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        list.Add(value.Trim());
                    }
                }
            }

            return new ReadOnlyCollection<string>(list);
        }
    }

    public sealed class ModuleContext
    {
        public ModuleContext(IMajoLogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IMajoLogger Logger { get; }
    }

    public interface IMajoModule
    {
        ModuleDescriptor Descriptor { get; }
        void Initialize(ModuleContext context);
        void Start();
        void Stop();
    }

    public sealed class ModuleSnapshot
    {
        public ModuleSnapshot(ModuleDescriptor descriptor, ModuleLifecycleState state, string failure)
        {
            Descriptor = descriptor;
            State = state;
            Failure = failure;
        }

        public ModuleDescriptor Descriptor { get; }
        public ModuleLifecycleState State { get; }
        public string Failure { get; }
    }
}
