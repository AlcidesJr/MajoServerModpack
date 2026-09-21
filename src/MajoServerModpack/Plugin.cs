using System;
using BepInEx;
using MajoServerModpack.Core.Diagnostics;
using MajoServerModpack.Core.Runtime;
using MajoServerModpack.Platform.Framework;
using MajoServerModpack.Platform.Game;
using MajoServerModpack.Platform.Network;
using Jotunn.Utils;

namespace MajoServerModpack
{
    [BepInPlugin(PluginGuid, PluginName, MajoVersions.MajoVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.None)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.majo.servermodpack";
        public const string PluginName = "MajoServerModpack";

        private MajoRuntime _runtime;
        private ValheimSecureRpcTransport _network;

        private void Awake()
        {
            var log = new BepInExMajoLogger(Logger);

            try
            {
                var metadata = FrameworkRuntimeMetadata.Create();
                var contextProvider = new ValheimExecutionContextProvider(log);

                _runtime = new MajoRuntime(log, contextProvider, metadata);
                _network = new ValheimSecureRpcTransport(
                    log,
                    _runtime.SecureRpc,
                    _runtime.Patches);
                _network.Install();

                var diagnostics = _runtime.Bootstrap();

                DiagnosticsReporter.Write(log, diagnostics);
            }
            catch (Exception exception)
            {
                _network?.Dispose();
                _network = null;
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

            _network?.Dispose();
            _network = null;
            _runtime.Shutdown();
            _runtime = null;
        }
    }
}
