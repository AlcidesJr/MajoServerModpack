using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MajoServerModpack.Core.Networking
{
    public enum GatewayExecutionSide
    {
        Client = 1,
        Server = 2
    }

    public enum RpcDirection
    {
        ClientToServer = 1,
        ServerToClient = 2,
        ServerBroadcast = 3
    }

    public enum MajoMessageType : byte
    {
        Hello = 1,
        HelloAck = 2,
        Request = 3,
        Response = 4,
        Error = 5
    }

    public enum RpcResultCode
    {
        Success = 0,
        Unauthorized = 1,
        Forbidden = 2,
        InvalidPayload = 3,
        UnsupportedProtocol = 4,
        RateLimited = 5,
        DuplicateRequest = 6,
        UnknownOperation = 7,
        HandlerFailed = 8,
        Unavailable = 9
    }

    public enum HandshakeState
    {
        Pending = 0,
        Compatible = 1,
        Rejected = 2
    }

    public enum AuditPolicy
    {
        None = 0,
        Failures = 1,
        All = 2
    }

    public static class MajoProtocol
    {
        public const int Magic = unchecked((int)0x4F4A414D);
        public const byte WireFormatVersion = 1;
        public const int HeaderSize = 26;
        public const int MaxEnvelopeSize = 64 * 1024;
        public const int MaxPayloadSize = MaxEnvelopeSize - HeaderSize;
        public const int MaxHelloCapabilities = 16;
        public const int MaxCapabilityLength = 64;
        public const int MaxVersionLength = 32;
        public const int MaxPublicErrorLength = 160;
        public const int HandshakeOperationId = 0;
        public const string CoreNetworkCapability = "Core.Network.v1";
    }

    public static class MajoPermissions
    {
        public const string None = "majo.none";
        public const string Player = "majo.player";
        public const string Admin = "majo.admin";
        public const string System = "majo.system";
    }

    public sealed class MajoMessageEnvelope
    {
        public MajoMessageEnvelope(
            MajoMessageType messageType,
            int protocolVersion,
            int operationId,
            long requestId,
            ArraySegment<byte> payload)
        {
            MessageType = messageType;
            ProtocolVersion = protocolVersion;
            OperationId = operationId;
            RequestId = requestId;
            Payload = payload.Array == null
                ? new ArraySegment<byte>(Array.Empty<byte>())
                : payload;
        }

        public MajoMessageType MessageType { get; }
        public int ProtocolVersion { get; }
        public int OperationId { get; }
        public long RequestId { get; }
        public ArraySegment<byte> Payload { get; }
    }

    public sealed class MajoHelloPayload
    {
        private readonly IReadOnlyList<string> _capabilities;

        public MajoHelloPayload(
            GatewayExecutionSide executionSide,
            string majoVersion,
            IEnumerable<string> capabilities)
        {
            if (!IsValidExecutionSide(executionSide))
            {
                throw new ArgumentOutOfRangeException(nameof(executionSide));
            }

            if (string.IsNullOrWhiteSpace(majoVersion))
            {
                throw new ArgumentException("Majo version is required.", nameof(majoVersion));
            }

            ExecutionSide = executionSide;
            MajoVersion = majoVersion.Trim();

            var normalized = new List<string>();
            if (capabilities != null)
            {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (var capability in capabilities)
                {
                    if (string.IsNullOrWhiteSpace(capability))
                    {
                        continue;
                    }

                    var value = capability.Trim();
                    if (seen.Add(value))
                    {
                        normalized.Add(value);
                    }
                }
            }

            _capabilities = new ReadOnlyCollection<string>(normalized);
        }

        public GatewayExecutionSide ExecutionSide { get; }
        public string MajoVersion { get; }
        public IReadOnlyList<string> Capabilities => _capabilities;

        public bool HasCapability(string capability)
        {
            if (string.IsNullOrEmpty(capability))
            {
                return false;
            }

            for (var index = 0; index < _capabilities.Count; index++)
            {
                if (string.Equals(_capabilities[index], capability, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsValidExecutionSide(GatewayExecutionSide side)
        {
            return side == GatewayExecutionSide.Client || side == GatewayExecutionSide.Server;
        }
    }

    public sealed class TrustedPeerContext
    {
        private readonly HashSet<string> _grantedPermissions;

        internal TrustedPeerContext(
            string connectionId,
            long peerId,
            string actorId,
            bool isAdmin,
            bool isSystem,
            bool isServerPeer,
            IEnumerable<string> grantedPermissions = null)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                throw new ArgumentException("Connection id is required.", nameof(connectionId));
            }

            ConnectionId = connectionId.Trim();
            PeerId = peerId;
            ActorId = string.IsNullOrWhiteSpace(actorId) ? "unknown" : actorId.Trim();
            IsAdmin = isAdmin;
            IsSystem = isSystem;
            IsServerPeer = isServerPeer;
            _grantedPermissions = new HashSet<string>(StringComparer.Ordinal);

            if (grantedPermissions != null)
            {
                foreach (var permission in grantedPermissions)
                {
                    if (!string.IsNullOrWhiteSpace(permission))
                    {
                        _grantedPermissions.Add(permission.Trim());
                    }
                }
            }
        }

        public string ConnectionId { get; }
        public long PeerId { get; }
        public string ActorId { get; }
        public bool IsAdmin { get; }
        public bool IsSystem { get; }
        public bool IsServerPeer { get; }

        public bool HasGrantedPermission(string permissionId)
        {
            return !string.IsNullOrEmpty(permissionId) && _grantedPermissions.Contains(permissionId);
        }
    }

    public sealed class MajoPeerSession
    {
        private const int ReplayWindowSize = 128;

        private readonly HashSet<string> _capabilities =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<long> _recentRequestIds = new HashSet<long>();
        private readonly Queue<long> _recentRequestOrder = new Queue<long>();

        internal MajoPeerSession(TrustedPeerContext peer)
        {
            if (peer == null)
            {
                throw new ArgumentNullException(nameof(peer));
            }

            ConnectionId = peer.ConnectionId;
            PeerId = peer.PeerId;
            HandshakeState = HandshakeState.Pending;
        }

        public string ConnectionId { get; }
        public long PeerId { get; }
        public HandshakeState HandshakeState { get; private set; }
        public int ProtocolVersion { get; private set; }
        public bool PeerReady { get; private set; }

        public bool TryCompleteHandshake(int protocolVersion, IEnumerable<string> capabilities)
        {
            if (HandshakeState != HandshakeState.Pending)
            {
                return false;
            }

            ProtocolVersion = protocolVersion;
            _capabilities.Clear();

            if (capabilities != null)
            {
                foreach (var capability in capabilities)
                {
                    if (!string.IsNullOrWhiteSpace(capability))
                    {
                        _capabilities.Add(capability.Trim());
                    }
                }
            }

            HandshakeState = HandshakeState.Compatible;
            return true;
        }

        public void RejectHandshake()
        {
            HandshakeState = HandshakeState.Rejected;
            ProtocolVersion = 0;
            PeerReady = false;
            _capabilities.Clear();
            _recentRequestIds.Clear();
            _recentRequestOrder.Clear();
        }

        public bool HasCapability(string capability)
        {
            return !string.IsNullOrEmpty(capability) && _capabilities.Contains(capability);
        }

        internal bool TryMarkPeerReady()
        {
            if (HandshakeState != HandshakeState.Compatible)
            {
                return false;
            }

            PeerReady = true;
            return true;
        }

        public bool TryAcceptRequest(long requestId)
        {
            if (requestId <= 0 || HandshakeState != HandshakeState.Compatible)
            {
                return false;
            }

            if (!_recentRequestIds.Add(requestId))
            {
                return false;
            }

            _recentRequestOrder.Enqueue(requestId);
            while (_recentRequestOrder.Count > ReplayWindowSize)
            {
                _recentRequestIds.Remove(_recentRequestOrder.Dequeue());
            }

            return true;
        }
    }

    public sealed class PeerSessionRegistry
    {
        private readonly Dictionary<string, MajoPeerSession> _sessions =
            new Dictionary<string, MajoPeerSession>(StringComparer.Ordinal);

        public int Count => _sessions.Count;

        public MajoPeerSession Create(TrustedPeerContext peer)
        {
            if (peer == null)
            {
                throw new ArgumentNullException(nameof(peer));
            }

            if (_sessions.ContainsKey(peer.ConnectionId))
            {
                throw new InvalidOperationException(
                    "Connection '" + peer.ConnectionId + "' is already registered.");
            }

            var session = new MajoPeerSession(peer);
            _sessions.Add(peer.ConnectionId, session);
            return session;
        }

        public bool TryGet(string connectionId, out MajoPeerSession session)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                session = null;
                return false;
            }

            return _sessions.TryGetValue(connectionId, out session);
        }

        public bool Remove(string connectionId)
        {
            return !string.IsNullOrEmpty(connectionId) && _sessions.Remove(connectionId);
        }

        public void Clear()
        {
            _sessions.Clear();
        }
    }

    public sealed class PayloadValidationResult
    {
        private PayloadValidationResult(bool isValid, string reason)
        {
            IsValid = isValid;
            Reason = reason ?? string.Empty;
        }

        public bool IsValid { get; }
        public string Reason { get; }

        public static PayloadValidationResult Valid()
        {
            return new PayloadValidationResult(true, string.Empty);
        }

        public static PayloadValidationResult Invalid(string reason)
        {
            return new PayloadValidationResult(false, reason);
        }
    }

    public sealed class RpcHandlerResult
    {
        private RpcHandlerResult(RpcResultCode code, byte[] payload, string publicMessage)
        {
            Code = code;
            Payload = payload ?? Array.Empty<byte>();
            PublicMessage = publicMessage ?? string.Empty;
        }

        public RpcResultCode Code { get; }
        public byte[] Payload { get; }
        public string PublicMessage { get; }

        public static RpcHandlerResult Success(byte[] payload = null)
        {
            return new RpcHandlerResult(RpcResultCode.Success, payload, string.Empty);
        }

        public static RpcHandlerResult Fail(RpcResultCode code, string publicMessage = null)
        {
            if (code == RpcResultCode.Success)
            {
                throw new ArgumentException("Failure result cannot use Success.", nameof(code));
            }

            return new RpcHandlerResult(code, Array.Empty<byte>(), publicMessage);
        }
    }

    public sealed class RpcRequestContext
    {
        internal RpcRequestContext(TrustedPeerContext peer, MajoPeerSession session, int operationId)
        {
            Peer = peer;
            Session = session;
            OperationId = operationId;
        }

        public TrustedPeerContext Peer { get; }
        public MajoPeerSession Session { get; }
        public int OperationId { get; }
    }

    public delegate PayloadValidationResult RpcPayloadValidator(ArraySegment<byte> payload);
    public delegate RpcHandlerResult RpcOperationHandler(
        RpcRequestContext context,
        ArraySegment<byte> payload);

    public sealed class RateLimitPolicy
    {
        public RateLimitPolicy(int requestsPerWindow, int burst, TimeSpan window)
        {
            if (requestsPerWindow <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requestsPerWindow));
            }

            if (burst < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(burst));
            }

            if (window <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(window));
            }

            RequestsPerWindow = requestsPerWindow;
            Burst = burst;
            Window = window;
        }

        public int RequestsPerWindow { get; }
        public int Burst { get; }
        public TimeSpan Window { get; }
        public int Capacity => RequestsPerWindow + Burst;
    }

    public sealed class OperationDescriptor
    {
        public OperationDescriptor(
            int operationId,
            RpcDirection direction,
            GatewayExecutionSide allowedExecutionSide,
            string requiredPermission,
            int maxPayloadSize,
            RateLimitPolicy rateLimitPolicy,
            AuditPolicy auditPolicy,
            int protocolVersion,
            RpcPayloadValidator validator,
            RpcOperationHandler handler)
        {
            if (operationId <= MajoProtocol.HandshakeOperationId)
            {
                throw new ArgumentOutOfRangeException(nameof(operationId));
            }

            if (!IsValidDirection(direction))
            {
                throw new ArgumentOutOfRangeException(nameof(direction));
            }

            if (!MajoHelloPayload.IsValidExecutionSide(allowedExecutionSide))
            {
                throw new ArgumentOutOfRangeException(nameof(allowedExecutionSide));
            }

            if (!DirectionMatchesExecutionSide(direction, allowedExecutionSide))
            {
                throw new ArgumentException(
                    "RPC direction does not match the allowed execution side.",
                    nameof(allowedExecutionSide));
            }

            if (string.IsNullOrWhiteSpace(requiredPermission))
            {
                throw new ArgumentException("Required permission is required.", nameof(requiredPermission));
            }

            if (maxPayloadSize < 0 || maxPayloadSize > MajoProtocol.MaxPayloadSize)
            {
                throw new ArgumentOutOfRangeException(nameof(maxPayloadSize));
            }

            if (rateLimitPolicy == null)
            {
                throw new ArgumentNullException(nameof(rateLimitPolicy));
            }

            if (protocolVersion <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(protocolVersion));
            }

            Validator = validator ?? throw new ArgumentNullException(nameof(validator));
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));

            OperationId = operationId;
            Direction = direction;
            AllowedExecutionSide = allowedExecutionSide;
            RequiredPermission = requiredPermission.Trim();
            MaxPayloadSize = maxPayloadSize;
            RateLimitPolicy = rateLimitPolicy;
            AuditPolicy = auditPolicy;
            ProtocolVersion = protocolVersion;
        }

        public int OperationId { get; }
        public RpcDirection Direction { get; }
        public GatewayExecutionSide AllowedExecutionSide { get; }
        public string RequiredPermission { get; }
        public int MaxPayloadSize { get; }
        public RateLimitPolicy RateLimitPolicy { get; }
        public AuditPolicy AuditPolicy { get; }
        public int ProtocolVersion { get; }
        public RpcPayloadValidator Validator { get; }
        public RpcOperationHandler Handler { get; }

        private static bool IsValidDirection(RpcDirection direction)
        {
            return direction == RpcDirection.ClientToServer ||
                   direction == RpcDirection.ServerToClient ||
                   direction == RpcDirection.ServerBroadcast;
        }

        private static bool DirectionMatchesExecutionSide(
            RpcDirection direction,
            GatewayExecutionSide side)
        {
            if (direction == RpcDirection.ClientToServer)
            {
                return side == GatewayExecutionSide.Server;
            }

            return side == GatewayExecutionSide.Client;
        }
    }

    public sealed class OperationRegistry
    {
        private readonly Dictionary<int, OperationDescriptor> _operations =
            new Dictionary<int, OperationDescriptor>();

        public int Count => _operations.Count;

        public void Register(OperationDescriptor operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (_operations.ContainsKey(operation.OperationId))
            {
                throw new InvalidOperationException(
                    "Operation id '" + operation.OperationId + "' is already registered.");
            }

            _operations.Add(operation.OperationId, operation);
        }

        public bool TryGet(int operationId, out OperationDescriptor operation)
        {
            return _operations.TryGetValue(operationId, out operation);
        }
    }

    public static class PayloadRules
    {
        public static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        public static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        public static bool IsCountWithin(int count, int maximum)
        {
            return count >= 0 && maximum >= 0 && count <= maximum;
        }

        public static bool IsStringWithin(string value, int maximumLength, bool allowEmpty = true)
        {
            if (value == null || maximumLength < 0)
            {
                return false;
            }

            if (!allowEmpty && value.Length == 0)
            {
                return false;
            }

            return value.Length <= maximumLength;
        }
    }
}
