using System;
using System.Reflection;
using Jotunn;
using Jotunn.Managers;
using MajoServerModpack.Core.Logging;
using MajoServerModpack.Core.Runtime;

namespace MajoServerModpack.Platform.Game
{
    internal sealed class ValheimExecutionContextProvider : IExecutionContextProvider
    {
        private static readonly FieldInfo OpenServerField =
            typeof(ZNet).GetField("m_openServer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private readonly IMajoLogger _logger;
        private bool _reportedMissingListenSignal;

        public ValheimExecutionContextProvider(IMajoLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ExecutionContextSnapshot Detect()
        {
            if (GUIManager.IsHeadless())
            {
                return new ExecutionContextSnapshot(
                    ExecutionContextKind.DedicatedServer,
                    "headless signal");
            }

            var znet = ZNet.instance;
            if (znet == null)
            {
                return new ExecutionContextSnapshot(
                    ExecutionContextKind.MenuOrPreWorld,
                    "ZNet not initialized");
            }

            if (znet.IsServerInstance())
            {
                return new ExecutionContextSnapshot(
                    ExecutionContextKind.DedicatedServer,
                    "ZNet dedicated server");
            }

            if (znet.IsClientInstance())
            {
                return new ExecutionContextSnapshot(
                    ExecutionContextKind.ConnectedClient,
                    "ZNet client");
            }

            if (znet.IsLocalInstance())
            {
                bool openServer;
                if (TryReadOpenServer(znet, out openServer))
                {
                    return new ExecutionContextSnapshot(
                        openServer ? ExecutionContextKind.ListenServer : ExecutionContextKind.LocalWorld,
                        openServer ? "local server open to peers" : "local world");
                }

                if (!_reportedMissingListenSignal)
                {
                    _reportedMissingListenSignal = true;
                    _logger.Warning(
                        "RuntimeContext",
                        "Valheim hosting signal is unavailable; reporting LocalWorld instead of guessing listen-server state.");
                }

                return new ExecutionContextSnapshot(
                    ExecutionContextKind.LocalWorld,
                    "hosting signal unavailable");
            }

            return new ExecutionContextSnapshot(ExecutionContextKind.Unknown, "unrecognized ZNet state");
        }

        private static bool TryReadOpenServer(ZNet znet, out bool openServer)
        {
            openServer = false;
            if (OpenServerField == null || OpenServerField.FieldType != typeof(bool))
            {
                return false;
            }

            try
            {
                openServer = (bool)OpenServerField.GetValue(znet);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
