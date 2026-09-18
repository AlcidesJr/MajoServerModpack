using System;
using BepInEx;
using MajoServerModpack.Core.Diagnostics;
using MajoServerModpack.Core.Runtime;
using MajoServerModpack.Platform.Framework;
using MajoServerModpack.Platform.Game;

namespace MajoServerModpack
{
    [BepInPlugin(PluginGuid, PluginName, MajoVersions.MajoVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.majo.servermodpack";
        public const string PluginName = "MajoServerModpack";

        private MajoRuntime _runtime;

        private void Awake()
        {
            var log = new BepInExMajoLogger(Logger);

            try
            {
                var metadata = FrameworkRuntimeMetadata.Create();
                var contextProvider = new ValheimExecutionContextProvider(log);

                _runtime = new MajoRuntime(log, contextProvider, metadata);
                var diagnostics = _runtime.Bootstrap();

                DiagnosticsReporter.Write(log, diagnostics);
            }
            catch (Exception exception)
            {
                log.Error("Bootstrap", "Core runtime bootstrap failed.", exception);
                throw;
            }
        }

        private void OnDestroy()
        {
            if (_runtime == null)
            {
                return;
            }

            _runtime.Shutdown();
            _runtime = null;
        }
    }
}
