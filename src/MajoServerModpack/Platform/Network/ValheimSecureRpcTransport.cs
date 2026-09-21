using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using HarmonyLib;
using MajoServerModpack.Core.Logging;
using MajoServerModpack.Core.Networking;
using MajoServerModpack.Core.Patching;

namespace MajoServerModpack.Platform.Network
{
    internal sealed class ValheimSecureRpcTransport : IDisposable
    {
        private const string RpcName = "com.majo.servermodpack.secure-rpc";
        private const string HarmonyId = "com.majo.servermodpack.secure-network";
        private const string PatchOwner = "Core.SecureRpcGateway";

        private sealed class PeerBinding
        {
            public PeerBinding(string connectionId, ZNetPeer peer)
            {
                ConnectionId = connectionId;
                Peer = peer;
            }

            public string ConnectionId { get; }
            public ZNetPeer Peer { get; }
        }

        private static ValheimSecureRpcTransport _current;

        private readonly IMajoLogger _logger;
        private readonly SecureRpcGateway _gateway;
        private readonly PatchCoordinator _patches;
        private readonly Harmony _harmony;
        private readonly Dictionary<ZRpc, PeerBinding> _bindings =
            new Dictionary<ZRpc, PeerBinding>();
        private readonly HashSet<string> _transportViolationLogged =
            new HashSet<string>(StringComparer.Ordinal);

        private long _connectionSequence;
        private long _requestSequence;
        private bool _installed;
        private bool _disposed;
        private string _lastConnectionError = string.Empty;

        public ValheimSecureRpcTransport(
            IMajoLogger logger,
            SecureRpcGateway gateway,
            PatchCoordinator patches)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
            _patches = patches ?? throw new ArgumentNullException(nameof(patches));
            _harmony = new Harmony(HarmonyId);
        }

        public void Install()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ValheimSecureRpcTransport));
            }

            if (_installed)
            {
                return;
            }

            if (_current != null)
            {
                throw new InvalidOperationException("Majo secure transport is already installed.");
            }

            RegisterPatchOwnership();
            _current = this;

            try
            {
                _harmony.PatchAll(typeof(Patches));
                _installed = true;
                _logger.Info(
                    "SecureRpc",
                    "Direct peer-bound RPC transport installed. Protocol=" +
                    _gateway.ProtocolVersion + ".");
            }
            catch
            {
                _current = null;
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_current == this)
            {
                _current = null;
            }

            var bindings = new List<PeerBinding>(_bindings.Values);
            for (var index = 0; index < bindings.Count; index++)
            {
                _gateway.Disconnect(bindings[index].ConnectionId);
            }

            _bindings.Clear();
            _transportViolationLogged.Clear();

            if (_installed)
            {
                _harmony.UnpatchSelf();
                _installed = false;
            }
        }

        private void RegisterPatchOwnership()
        {
            _patches.RegisterOwner("ZNet.OnNewConnection", PatchOwner);
            _patches.RegisterOwner("ZNet.RPC_ClientHandshake", PatchOwner);
            _patches.RegisterOwner("ZNet.RPC_PeerInfo", PatchOwner);
            _patches.RegisterOwner("ZNet.Disconnect(ZNetPeer)", PatchOwner);
            _patches.RegisterOwner("FejdStartup.ShowConnectError", PatchOwner);
        }

        private void BindPeer(ZNetPeer peer)
        {
            if (peer == null || peer.m_rpc == null || _bindings.ContainsKey(peer.m_rpc))
            {
                return;
            }

            var connectionId =
                "conn-" +
                Interlocked.Increment(ref _connectionSequence)
                    .ToString(CultureInfo.InvariantCulture);
            var binding = new PeerBinding(connectionId, peer);

            peer.m_rpc.Register<ZPackage>(RpcName, OnReceive);
            _bindings.Add(peer.m_rpc, binding);
            _gateway.Connect(CreateTrustedContext(binding));

            if (ZNet.instance != null && !ZNet.instance.IsServer())
            {
                _lastConnectionError = string.Empty;
            }

            _logger.Debug(
                "SecureRpc",
                "Peer RPC bound: connection=" + connectionId + ".");
        }

        private void UnbindPeer(ZNetPeer peer)
        {
            if (peer == null || peer.m_rpc == null)
            {
                return;
            }

            if (!_bindings.TryGetValue(peer.m_rpc, out var binding))
            {
                return;
            }

            _bindings.Remove(peer.m_rpc);
            _transportViolationLogged.Remove(binding.ConnectionId);
            _gateway.Disconnect(binding.ConnectionId);

            _logger.Debug(
                "SecureRpc",
                "Peer RPC unbound: connection=" + binding.ConnectionId + ".");
        }

        private void SendClientHello(ZRpc rpc)
        {
            if (rpc == null || !_bindings.TryGetValue(rpc, out var binding))
            {
                return;
            }

            var requestId = Interlocked.Increment(ref _requestSequence);
            if (requestId <= 0)
            {
                Interlocked.Exchange(ref _requestSequence, 1);
                requestId = 1;
            }

            var hello = _gateway.CreateHello(
                GatewayExecutionSide.Client,
                requestId);
            rpc.Invoke(RpcName, new ZPackage(hello));

            _logger.Debug(
                "SecureRpc",
                "Client hello sent: connection=" + binding.ConnectionId + ".");
        }

        private void OnReceive(ZRpc sender, ZPackage package)
        {
            if (sender == null || !_bindings.TryGetValue(sender, out var binding))
            {
                return;
            }

            var peer = CreateTrustedContext(binding);
            var localSide =
                ZNet.instance != null && ZNet.instance.IsServer()
                    ? GatewayExecutionSide.Server
                    : GatewayExecutionSide.Client;

            if (package == null ||
                package.Size() <= 0 ||
                package.Size() > MajoProtocol.MaxEnvelopeSize)
            {
                var violation = _gateway.ReportTransportViolation(
                    peer,
                    RpcResultCode.InvalidPayload,
                    "Direct RPC package is null, empty, or oversized.");

                if (_transportViolationLogged.Add(binding.ConnectionId))
                {
                    _logger.Warning(
                        "SecureRpc",
                        "Rejected invalid direct RPC package for connection=" +
                        binding.ConnectionId + ".");
                }

                HandleDisconnect(sender, localSide, violation);
                return;
            }

            byte[] data;
            try
            {
                data = package.GetArray();
            }
            catch (Exception exception)
            {
                var violation = _gateway.ReportTransportViolation(
                    peer,
                    RpcResultCode.InvalidPayload,
                    "Direct RPC package could not be materialized.");
                _logger.Error(
                    "SecureRpc",
                    "Failed to read direct RPC package.",
                    exception);
                HandleDisconnect(sender, localSide, violation);
                return;
            }

            var result = _gateway.ProcessIncoming(peer, localSide, data);

            if (result.ShouldRespond)
            {
                sender.Invoke(RpcName, new ZPackage(result.ResponseBytes));
            }

            if (localSide == GatewayExecutionSide.Client &&
                result.Code != RpcResultCode.Success &&
                !string.IsNullOrWhiteSpace(result.PublicMessage))
            {
                _lastConnectionError = result.PublicMessage;
                _logger.Warning(
                    "SecureRpc",
                    "Server rejected Majo networking: " + result.PublicMessage);
            }

            HandleDisconnect(sender, localSide, result);
        }

        private void HandleDisconnect(
            ZRpc sender,
            GatewayExecutionSide localSide,
            GatewayDispatchResult result)
        {
            if (sender == null || result == null || !result.DisconnectPeer)
            {
                return;
            }

            if (localSide == GatewayExecutionSide.Server)
            {
                sender.Invoke(
                    "Error",
                    (int)ZNet.ConnectionStatus.ErrorVersion);

                if (_bindings.TryGetValue(sender, out var binding) &&
                    ZNet.instance != null)
                {
                    ZNet.instance.Disconnect(binding.Peer);
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(_lastConnectionError))
            {
                _lastConnectionError = string.IsNullOrWhiteSpace(result.PublicMessage)
                    ? "Majo network compatibility check failed."
                    : result.PublicMessage;
            }

            ZNet.m_connectionStatus = ZNet.ConnectionStatus.ErrorVersion;
            sender.Invoke("Disconnect");

            if (_bindings.TryGetValue(sender, out var binding) &&
                ZNet.instance != null)
            {
                ZNet.instance.Disconnect(binding.Peer);
            }
        }

        private void MarkPeerReady(ZNet instance, ZRpc rpc)
        {
            if (instance == null ||
                !instance.IsServer() ||
                rpc == null ||
                !_bindings.TryGetValue(rpc, out var binding) ||
                binding.Peer == null ||
                !binding.Peer.IsReady())
            {
                return;
            }

            if (_gateway.MarkPeerReady(binding.ConnectionId))
            {
                _logger.Debug(
                    "SecureRpc",
                    "Peer authenticated and ready: connection=" +
                    binding.ConnectionId + ".");
            }
        }

        private bool AllowPeerInfo(ZNet instance, ZRpc rpc)
        {
            if (instance == null || !instance.IsServer())
            {
                return true;
            }

            if (rpc == null ||
                !_bindings.TryGetValue(rpc, out var binding) ||
                !_gateway.IsCompatible(binding.ConnectionId))
            {
                if (rpc != null)
                {
                    rpc.Invoke(
                        "Error",
                        (int)ZNet.ConnectionStatus.ErrorVersion);
                }

                _logger.Warning(
                    "SecureRpc",
                    "Rejected peer before PeerInfo because Majo handshake is not compatible.");
                return false;
            }

            return true;
        }

        private void AppendConnectionError(FejdStartup startup)
        {
            if (startup == null ||
                startup.m_connectionFailedError == null ||
                string.IsNullOrWhiteSpace(_lastConnectionError))
            {
                return;
            }

            var suffix = Environment.NewLine + "Majo: " + _lastConnectionError;
            if (!startup.m_connectionFailedError.text.Contains(suffix))
            {
                startup.m_connectionFailedError.text += suffix;
            }
        }

        private TrustedPeerContext CreateTrustedContext(PeerBinding binding)
        {
            var znet = ZNet.instance;
            var isServer = znet != null && znet.IsServer();
            var isAdmin = false;

            if (isServer &&
                binding.Peer != null &&
                binding.Peer.m_socket != null &&
                znet.m_adminList != null)
            {
                var transportIdentity = binding.Peer.m_socket.GetHostName();
                isAdmin =
                    !string.IsNullOrEmpty(transportIdentity) &&
                    znet.ListContainsId(
                        znet.m_adminList,
                        transportIdentity);
            }

            var peerId = binding.Peer == null ? 0L : binding.Peer.m_uid;
            var actorId =
                "peer:" + peerId.ToString(CultureInfo.InvariantCulture);

            return new TrustedPeerContext(
                binding.ConnectionId,
                peerId,
                actorId,
                isAdmin,
                false,
                !isServer);
        }

        private static class Patches
        {
            [HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
            [HarmonyPrefix]
            [HarmonyPriority(Priority.First)]
            private static void ZNet_OnNewConnection_Prefix(ZNetPeer peer)
            {
                _current?.BindPeer(peer);
            }

            [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_ClientHandshake))]
            [HarmonyPrefix]
            [HarmonyPriority(Priority.First)]
            private static void ZNet_RPC_ClientHandshake_Prefix(ZRpc rpc)
            {
                if (ZNet.instance != null && !ZNet.instance.IsServer())
                {
                    _current?.SendClientHello(rpc);
                }
            }

            [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
            [HarmonyPrefix]
            [HarmonyPriority(Priority.First)]
            private static bool ZNet_RPC_PeerInfo_Prefix(
                ZNet __instance,
                ZRpc rpc)
            {
                return _current == null ||
                       _current.AllowPeerInfo(__instance, rpc);
            }

            [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
            [HarmonyPostfix]
            [HarmonyPriority(Priority.Last)]
            private static void ZNet_RPC_PeerInfo_Postfix(
                ZNet __instance,
                ZRpc rpc)
            {
                _current?.MarkPeerReady(__instance, rpc);
            }

            [HarmonyPatch(
                typeof(ZNet),
                nameof(ZNet.Disconnect),
                new Type[] { typeof(ZNetPeer) })]
            [HarmonyPrefix]
            private static void ZNet_Disconnect_Prefix(ZNetPeer peer)
            {
                _current?.UnbindPeer(peer);
            }

            [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.ShowConnectError))]
            [HarmonyPostfix]
            private static void FejdStartup_ShowConnectError_Postfix(
                FejdStartup __instance)
            {
                _current?.AppendConnectionError(__instance);
            }
        }
    }
}
