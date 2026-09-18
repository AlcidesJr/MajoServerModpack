using System;
using System.Diagnostics;
using MajoServerModpack.Core.Diagnostics;
using MajoServerModpack.Core.Input;
using MajoServerModpack.Core.Logging;
using MajoServerModpack.Core.Modules;
using MajoServerModpack.Core.Patching;

namespace MajoServerModpack.Core.Runtime
{
    public sealed class MajoRuntime
    {
        private readonly IMajoLogger _logger;
        private readonly IExecutionContextProvider _executionContextProvider;
        private bool _bootstrapped;
        private bool _shutdown;
        private TimeSpan _bootstrapDuration;

        public MajoRuntime(
            IMajoLogger logger,
            IExecutionContextProvider executionContextProvider,
            RuntimeMetadata metadata)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _executionContextProvider = executionContextProvider ?? throw new ArgumentNullException(nameof(executionContextProvider));
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));

            Modules = new ModuleRegistry(logger);
            Inputs = new InputRegistry();
            Patches = new PatchCoordinator();
        }

        public RuntimeMetadata Metadata { get; }
        public ModuleRegistry Modules { get; }
        public InputRegistry Inputs { get; }
        public PatchCoordinator Patches { get; }

        public DiagnosticsSnapshot Bootstrap()
        {
            if (_bootstrapped)
            {
                throw new InvalidOperationException("Majo runtime is already bootstrapped.");
            }

            var stopwatch = Stopwatch.StartNew();
            _logger.Info("Runtime", "Bootstrap starting.");

            Modules.InitializeAll();
            Modules.StartAll();

            stopwatch.Stop();
            _bootstrapDuration = stopwatch.Elapsed;
            _bootstrapped = true;

            _logger.Info("Runtime", "Bootstrap completed.");
            return CreateDiagnosticsSnapshot();
        }

        public ExecutionContextSnapshot DetectExecutionContext()
        {
            return _executionContextProvider.Detect();
        }

        public DiagnosticsSnapshot CreateDiagnosticsSnapshot()
        {
            return new DiagnosticsSnapshot(
                Metadata,
                DetectExecutionContext(),
                Modules.Snapshot(),
                Patches.Count,
                Inputs.Count,
                _bootstrapDuration);
        }

        public void Shutdown()
        {
            if (_shutdown)
            {
                return;
            }

            _shutdown = true;
            if (!_bootstrapped)
            {
                return;
            }

            _logger.Info("Runtime", "Shutdown starting.");
            Modules.StopAll();
            _logger.Info("Runtime", "Shutdown completed.");
        }
    }
}
