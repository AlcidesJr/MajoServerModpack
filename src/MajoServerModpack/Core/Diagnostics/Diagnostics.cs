using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MajoServerModpack.Core.Logging;
using MajoServerModpack.Core.Modules;
using MajoServerModpack.Core.Runtime;

namespace MajoServerModpack.Core.Diagnostics
{
    public sealed class DiagnosticsSnapshot
    {
        public DiagnosticsSnapshot(
            RuntimeMetadata metadata,
            ExecutionContextSnapshot executionContext,
            IReadOnlyList<ModuleSnapshot> modules,
            int patchSurfaceCount,
            int inputActionCount,
            TimeSpan bootstrapDuration)
        {
            Metadata = metadata;
            ExecutionContext = executionContext;
            Modules = modules ?? new ReadOnlyCollection<ModuleSnapshot>(new List<ModuleSnapshot>());
            PatchSurfaceCount = patchSurfaceCount;
            InputActionCount = inputActionCount;
            BootstrapDuration = bootstrapDuration;
        }

        public RuntimeMetadata Metadata { get; }
        public ExecutionContextSnapshot ExecutionContext { get; }
        public IReadOnlyList<ModuleSnapshot> Modules { get; }
        public int PatchSurfaceCount { get; }
        public int InputActionCount { get; }
        public TimeSpan BootstrapDuration { get; }

        public int ActiveModuleCount
        {
            get
            {
                var count = 0;
                foreach (var module in Modules)
                {
                    if (module.State == ModuleLifecycleState.Started)
                    {
                        count++;
                    }
                }

                return count;
            }
        }
    }

    public static class DiagnosticsReporter
    {
        public static void Write(IMajoLogger logger, DiagnosticsSnapshot snapshot)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            var metadata = snapshot.Metadata;
            logger.Info("Diagnostics",
                "Majo=" + metadata.MajoVersion +
                " Protocol=" + metadata.ProtocolVersion +
                " ConfigSchema=" + metadata.ConfigSchema +
                " DataSchema=" + metadata.DataSchema);
            logger.Info("Diagnostics",
                "Valheim=" + metadata.ValheimVersion +
                " BepInEx=" + metadata.BepInExVersion +
                " Jotunn=" + metadata.JotunnVersion);
            logger.Info("Diagnostics", "ExecutionContext=" + snapshot.ExecutionContext);
            logger.Info("Diagnostics",
                "Modules=" + snapshot.Modules.Count +
                " Active=" + snapshot.ActiveModuleCount +
                " PatchSurfaces=" + snapshot.PatchSurfaceCount +
                " Inputs=" + snapshot.InputActionCount +
                " BootstrapMs=" + snapshot.BootstrapDuration.TotalMilliseconds.ToString("F1"));
        }
    }
}
