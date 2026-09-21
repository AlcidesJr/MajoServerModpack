using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MajoServerModpack.Core.Logging;

namespace MajoServerModpack.Core.Networking
{
    public sealed class GatewayDispatchResult
    {
        internal GatewayDispatchResult(
            RpcResultCode code,
            byte[] responseBytes,
            bool handlerInvoked,
            bool disconnectPeer,
            string publicMessage)
        {
            Code = code;
            ResponseBytes = responseBytes;
            HandlerInvoked = handlerInvoked;
            DisconnectPeer = disconnectPeer;
            PublicMessage = publicMessage ?? string.Empty;
        }

        public RpcResultCode Code { get; }
        public byte[] ResponseBytes { get; }
        public bool ShouldRespond => ResponseBytes != null && ResponseBytes.Length > 0;
        public bool HandlerInvoked { get; }
        public bool DisconnectPeer { get; }
        public string PublicMessage { get; }
    }

    public sealed class SecureRpcGateway
    {
        private const int InvalidTrafficOperationId = int.MinValue;
        private const int GlobalPeerOperationId = int.MinValue + 1;
        private const int HandshakeRateOperationId = int.MinValue + 2;

        private static readonly RateLimitPolicy InvalidTrafficPolicy =
            new RateLimitPolicy(5, 0, TimeSpan.FromSeconds(10));
        private static readonly RateLimitPolicy GlobalPeerPolicy =
            new RateLimitPolicy(60, 20, TimeSpan.FromSeconds(1));
        private static readonly RateLimitPolicy HandshakeRatePolicy =
            new RateLimitPolicy(2, 0, TimeSpan.FromSeconds(10));

        private readonly IMajoLogger _logger;
        private readonly PermissionAuthorizer _authorizer;
        private readonly FixedWindowRateLimiter _rateLimiter;
        private readonly IAuditSink _auditSink;
        private readonly IReadOnlyList<string> _requiredCapabilities;
        private readonly string _majoVersion;

        public SecureRpcGateway(
            IMajoLogger logger,
            string majoVersion,
            int protocolVersion,
            IEnumerable<string> requiredCapabilities,
            IRateLimitClock clock,
            IAuditSink auditSink)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrWhiteSpace(majoVersion))
            {
                throw new ArgumentException("Majo version is required.", nameof(majoVersion));
            }

            if (protocolVersion <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(protocolVersion));
            }

            _majoVersion = majoVersion.Trim();
            ProtocolVersion = protocolVersion;
            _authorizer = new PermissionAuthorizer();
            _rateLimiter = new FixedWindowRateLimiter(
                clock ?? throw new ArgumentNullException(nameof(clock)));
            _auditSink = auditSink ?? throw new ArgumentNullException(nameof(auditSink));

            var capabilities = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            AddCapability(
                MajoProtocol.CoreNetworkCapability,
                capabilities,
                seen);

            if (requiredCapabilities != null)
            {
                foreach (var capability in requiredCapabilities)
                {
                    AddCapability(capability, capabilities, seen);
                }
            }

            if (capabilities.Count > MajoProtocol.MaxHelloCapabilities)
            {
                throw new ArgumentException("Too many required capabilities.", nameof(requiredCapabilities));
            }

            _requiredCapabilities =
                new ReadOnlyCollection<string>(capabilities);

            Operations = new OperationRegistry();
            Sessions = new PeerSessionRegistry();
        }

        public int ProtocolVersion { get; }
        public OperationRegistry Operations { get; }
        public PeerSessionRegistry Sessions { get; }
        public IReadOnlyList<string> RequiredCapabilities => _requiredCapabilities;

        public void Register(OperationDescriptor operation)
        {
            Operations.Register(operation);
        }

        public MajoPeerSession Connect(TrustedPeerContext peer)
        {
            return Sessions.Create(peer);
        }

        public bool Disconnect(string connectionId)
        {
            _rateLimiter.ClearConnection(connectionId);
            return Sessions.Remove(connectionId);
        }

        public void Reset()
        {
            _rateLimiter.Clear();
            Sessions.Clear();
        }

        public bool IsCompatible(string connectionId)
        {
            return Sessions.TryGet(connectionId, out var session) &&
                   session.HandshakeState == HandshakeState.Compatible &&
                   session.ProtocolVersion == ProtocolVersion;
        }

        public GatewayDispatchResult ReportTransportViolation(
            TrustedPeerContext peer,
            RpcResultCode code,
            string reason)
        {
            if (peer == null)
            {
                throw new ArgumentNullException(nameof(peer));
            }

            AuditInvalid(
                peer,
                MajoProtocol.HandshakeOperationId,
                code,
                reason);

            return Result(
                code,
                null,
                false,
                false,
                "Invalid Majo transport message.");
        }

        public byte[] CreateHello(GatewayExecutionSide localSide, long requestId)
        {
            if (!MajoHelloPayload.IsValidExecutionSide(localSide))
            {
                throw new ArgumentOutOfRangeException(nameof(localSide));
            }

            if (requestId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requestId));
            }

            var hello = new MajoHelloPayload(
                localSide,
                _majoVersion,
                _requiredCapabilities);

            return MajoMessageCodec.Encode(
                new MajoMessageEnvelope(
                    MajoMessageType.Hello,
                    ProtocolVersion,
                    MajoProtocol.HandshakeOperationId,
                    requestId,
                    new ArraySegment<byte>(MajoHelloCodec.Encode(hello))));
        }

        public byte[] CreateRequest(
            int operationId,
            long requestId,
            byte[] payload)
        {
            if (!Operations.TryGet(operationId, out var operation))
            {
                throw new InvalidOperationException(
                    "Operation id '" + operationId + "' is not registered.");
            }

            if (operation.ProtocolVersion != ProtocolVersion)
            {
                throw new InvalidOperationException(
                    "Operation protocol does not match gateway protocol.");
            }

            var bytes = payload ?? Array.Empty<byte>();
            if (bytes.Length > operation.MaxPayloadSize)
            {
                throw new ArgumentOutOfRangeException(nameof(payload));
            }

            if (requestId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requestId));
            }

            return MajoMessageCodec.Encode(
                new MajoMessageEnvelope(
                    MajoMessageType.Request,
                    ProtocolVersion,
                    operationId,
                    requestId,
                    new ArraySegment<byte>(bytes)));
        }

        public GatewayDispatchResult ProcessIncoming(
            TrustedPeerContext peer,
            GatewayExecutionSide localSide,
            byte[] data)
        {
            if (peer == null)
            {
                throw new ArgumentNullException(nameof(peer));
            }

            if (!MajoHelloPayload.IsValidExecutionSide(localSide))
            {
                throw new ArgumentOutOfRangeException(nameof(localSide));
            }

            if (!Sessions.TryGet(peer.ConnectionId, out var session))
            {
                AuditInvalid(
                    peer,
                    MajoProtocol.HandshakeOperationId,
                    RpcResultCode.Unauthorized,
                    "Unknown or mismatched connection.");
                return Result(
                    RpcResultCode.Unauthorized,
                    null,
                    false,
                    false,
                    "Unknown connection.");
            }

            if (!MajoMessageCodec.TryDecode(
                data,
                out var envelope,
                out var decodeCode,
                out var decodeError))
            {
                AuditInvalid(
                    peer,
                    MajoProtocol.HandshakeOperationId,
                    decodeCode,
                    decodeError);
                return Result(
                    decodeCode,
                    null,
                    false,
                    decodeCode == RpcResultCode.UnsupportedProtocol,
                    "Invalid Majo network message.");
            }

            if (envelope.ProtocolVersion != ProtocolVersion)
            {
                session.RejectHandshake();
                AuditInvalid(
                    peer,
                    envelope.OperationId,
                    RpcResultCode.UnsupportedProtocol,
                    "Protocol version mismatch.");

                return Result(
                    RpcResultCode.UnsupportedProtocol,
                    BuildErrorResponse(
                        envelope,
                        RpcResultCode.UnsupportedProtocol,
                        "Majo protocol is incompatible with this server."),
                    false,
                    true,
                    "Majo protocol is incompatible with this server.");
            }

            switch (envelope.MessageType)
            {
                case MajoMessageType.Hello:
                    return HandleHello(peer, session, localSide, envelope);

                case MajoMessageType.HelloAck:
                    return HandleHelloAck(peer, session, localSide, envelope);

                case MajoMessageType.Request:
                    return HandleRequest(peer, session, localSide, envelope);

                case MajoMessageType.Response:
                    return Result(
                        RpcResultCode.Success,
                        null,
                        false,
                        false,
                        string.Empty);

                case MajoMessageType.Error:
                    return HandleRemoteError(session, envelope);

                default:
                    AuditInvalid(
                        peer,
                        envelope.OperationId,
                        RpcResultCode.InvalidPayload,
                        "Unhandled message type.");
                    return Result(
                        RpcResultCode.InvalidPayload,
                        null,
                        false,
                        false,
                        "Invalid Majo network message.");
            }
        }

        private GatewayDispatchResult HandleHello(
            TrustedPeerContext peer,
            MajoPeerSession session,
            GatewayExecutionSide localSide,
            MajoMessageEnvelope envelope)
        {
            if (!_rateLimiter.TryConsume(
                peer.ConnectionId,
                HandshakeRateOperationId,
                HandshakeRatePolicy))
            {
                return Result(
                    RpcResultCode.RateLimited,
                    BuildErrorResponse(
                        envelope,
                        RpcResultCode.RateLimited,
                        "Too many handshake messages."),
                    false,
                    true,
                    "Too many handshake messages.");
            }

            if (localSide != GatewayExecutionSide.Server ||
                envelope.OperationId != MajoProtocol.HandshakeOperationId ||
                envelope.RequestId <= 0)
            {
                session.RejectHandshake();
                AuditInvalid(
                    peer,
                    envelope.OperationId,
                    RpcResultCode.InvalidPayload,
                    "Hello arrived on an invalid side or envelope.");
                return Result(
                    RpcResultCode.InvalidPayload,
                    BuildErrorResponse(
                        envelope,
                        RpcResultCode.InvalidPayload,
                        "Invalid Majo handshake."),
                    false,
                    true,
                    "Invalid Majo handshake.");
            }

            if (session.HandshakeState != HandshakeState.Pending)
            {
                AuditInvalid(
                    peer,
                    envelope.OperationId,
                    RpcResultCode.DuplicateRequest,
                    "Handshake already completed.");
                return Result(
                    RpcResultCode.DuplicateRequest,
                    BuildErrorResponse(
                        envelope,
                        RpcResultCode.DuplicateRequest,
                        "Majo handshake was already completed."),
                    false,
                    true,
                    "Majo handshake was already completed.");
            }

            if (!MajoHelloCodec.TryDecode(
                envelope.Payload,
                out var hello,
                out var helloError))
            {
                session.RejectHandshake();
                AuditInvalid(
                    peer,
                    envelope.OperationId,
                    RpcResultCode.InvalidPayload,
                    helloError);
                return Result(
                    RpcResultCode.InvalidPayload,
                    BuildErrorResponse(
                        envelope,
                        RpcResultCode.InvalidPayload,
                        "Invalid Majo handshake."),
                    false,
                    true,
                    "Invalid Majo handshake.");
            }

            if (hello.ExecutionSide != GatewayExecutionSide.Client ||
                !HasRequiredCapabilities(hello))
            {
                session.RejectHandshake();
                AuditInvalid(
                    peer,
                    envelope.OperationId,
                    RpcResultCode.UnsupportedProtocol,
                    "Required capability missing or peer side invalid.");
                return Result(
                    RpcResultCode.UnsupportedProtocol,
                    BuildErrorResponse(
                        envelope,
                        RpcResultCode.UnsupportedProtocol,
                        "Required Majo network capability is missing."),
                    false,
                    true,
                    "Required Majo network capability is missing.");
            }

            if (!session.TryCompleteHandshake(
                ProtocolVersion,
                hello.Capabilities))
            {
                return Result(
                    RpcResultCode.DuplicateRequest,
                    BuildErrorResponse(
                        envelope,
                        RpcResultCode.DuplicateRequest,
                        "Majo handshake was already completed."),
                    false,
                    true,
                    "Majo handshake was already completed.");
            }

            RecordAudit(
                peer,
                envelope.OperationId,
                RpcResultCode.Success,
                "Handshake accepted.",
                AuditPolicy.All);

            var serverHello = new MajoHelloPayload(
                GatewayExecutionSide.Server,
                _majoVersion,
                _requiredCapabilities);
            var response = new MajoMessageEnvelope(
                MajoMessageType.HelloAck,
                ProtocolVersion,
                MajoProtocol.HandshakeOperationId,
                envelope.RequestId,
                new ArraySegment<byte>(MajoHelloCodec.Encode(serverHello)));

            return Result(
                RpcResultCode.Success,
                MajoMessageCodec.Encode(response),
                false,
                false,
                string.Empty);
        }

        private GatewayDispatchResult HandleHelloAck(
            TrustedPeerContext peer,
            MajoPeerSession session,
            GatewayExecutionSide localSide,
            MajoMessageEnvelope envelope)
        {
            if (localSide != GatewayExecutionSide.Client ||
                envelope.OperationId != MajoProtocol.HandshakeOperationId ||
                envelope.RequestId <= 0)
            {
                session.RejectHandshake();
                AuditInvalid(
                    peer,
                    envelope.OperationId,
                    RpcResultCode.InvalidPayload,
                    "HelloAck arrived on an invalid side or envelope.");
                return Result(
                    RpcResultCode.InvalidPayload,
                    null,
                    false,
                    true,
                    "Invalid Majo handshake response.");
            }

            if (session.HandshakeState != HandshakeState.Pending)
            {
                AuditInvalid(
                    peer,
                    envelope.OperationId,
                    RpcResultCode.DuplicateRequest,
                    "Handshake acknowledgement duplicated.");
                return Result(
                    RpcResultCode.DuplicateRequest,
                    null,
                    false,
                    true,
                    "Duplicate Majo handshake response.");
            }

            if (!MajoHelloCodec.TryDecode(
                envelope.Payload,
                out var hello,
                out var helloError) ||
                hello.ExecutionSide != GatewayExecutionSide.Server ||
                !HasRequiredCapabilities(hello))
            {
                session.RejectHandshake();
                AuditInvalid(
                    peer,
                    envelope.OperationId,
                    RpcResultCode.UnsupportedProtocol,
                    string.IsNullOrEmpty(helloError)
                        ? "Server capability missing or side invalid."
                        : helloError);
                return Result(
                    RpcResultCode.UnsupportedProtocol,
                    null,
                    false,
                    true,
                    "Majo server is not protocol-compatible.");
            }

            session.TryCompleteHandshake(
                ProtocolVersion,
                hello.Capabilities);

            RecordAudit(
                peer,
                envelope.OperationId,
                RpcResultCode.Success,
                "Handshake acknowledgement accepted.",
                AuditPolicy.All);

            return Result(
                RpcResultCode.Success,
                null,
                false,
                false,
                string.Empty);
        }

        private GatewayDispatchResult HandleRequest(
            TrustedPeerContext peer,
            MajoPeerSession session,
            GatewayExecutionSide localSide,
            MajoMessageEnvelope envelope)
        {
            if (session.HandshakeState != HandshakeState.Compatible ||
                session.ProtocolVersion != ProtocolVersion)
            {
                return RejectRequest(
                    peer,
                    envelope,
                    RpcResultCode.Unauthorized,
                    "Majo handshake is not complete.",
                    null,
                    false);
            }

            if (envelope.RequestId <= 0)
            {
                return RejectRequest(
                    peer,
                    envelope,
                    RpcResultCode.InvalidPayload,
                    "Request id is invalid.",
                    null,
                    false);
            }

            if (!Operations.TryGet(envelope.OperationId, out var operation))
            {
                AuditInvalid(
                    peer,
                    envelope.OperationId,
                    RpcResultCode.UnknownOperation,
                    "Unknown operation.");
                return Result(
                    RpcResultCode.UnknownOperation,
                    BuildErrorResponse(
                        envelope,
                        RpcResultCode.UnknownOperation,
                        "Unknown Majo operation."),
                    false,
                    false,
                    "Unknown Majo operation.");
            }

            if (operation.ProtocolVersion != ProtocolVersion)
            {
                return RejectRequest(
                    peer,
                    envelope,
                    RpcResultCode.UnsupportedProtocol,
                    "Operation protocol is unsupported.",
                    operation,
                    true);
            }

            if (operation.AllowedExecutionSide != localSide ||
                !DirectionMatchesLocalSide(operation.Direction, localSide))
            {
                return RejectRequest(
                    peer,
                    envelope,
                    RpcResultCode.Forbidden,
                    "Operation direction is not allowed on this side.",
                    operation,
                    false);
            }

            if (envelope.Payload.Count > operation.MaxPayloadSize)
            {
                return RejectRequest(
                    peer,
                    envelope,
                    RpcResultCode.InvalidPayload,
                    "Operation payload exceeds its maximum size.",
                    operation,
                    false);
            }

            if (!_rateLimiter.TryConsume(
                    peer.ConnectionId,
                    GlobalPeerOperationId,
                    GlobalPeerPolicy) ||
                !_rateLimiter.TryConsume(
                    peer.ConnectionId,
                    operation.OperationId,
                    operation.RateLimitPolicy))
            {
                return RejectRequest(
                    peer,
                    envelope,
                    RpcResultCode.RateLimited,
                    "Rate limit exceeded.",
                    operation,
                    false);
            }

            if (!session.TryAcceptRequest(envelope.RequestId))
            {
                return RejectRequest(
                    peer,
                    envelope,
                    RpcResultCode.DuplicateRequest,
                    "Request id was already processed.",
                    operation,
                    false);
            }

            var authorization = _authorizer.Authorize(
                peer,
                operation.RequiredPermission);
            if (authorization != RpcResultCode.Success)
            {
                return RejectRequest(
                    peer,
                    envelope,
                    authorization,
                    "Permission denied.",
                    operation,
                    false);
            }

            PayloadValidationResult validation;
            try
            {
                validation = operation.Validator(envelope.Payload);
            }
            catch (Exception exception)
            {
                _logger.Error(
                    "SecureRpc",
                    "Payload validator failed for operation " + operation.OperationId + ".",
                    exception);

                return RejectRequest(
                    peer,
                    envelope,
                    RpcResultCode.InvalidPayload,
                    "Payload validator failed.",
                    operation,
                    false);
            }

            if (validation == null || !validation.IsValid)
            {
                return RejectRequest(
                    peer,
                    envelope,
                    RpcResultCode.InvalidPayload,
                    validation == null
                        ? "Payload validator returned no result."
                        : validation.Reason,
                    operation,
                    false);
            }

            RpcHandlerResult handlerResult;
            try
            {
                handlerResult = operation.Handler(
                    new RpcRequestContext(peer, session, operation.OperationId),
                    envelope.Payload);
            }
            catch (Exception exception)
            {
                _logger.Error(
                    "SecureRpc",
                    "Handler failed for operation " + operation.OperationId + ".",
                    exception);

                RecordAudit(
                    peer,
                    operation.OperationId,
                    RpcResultCode.HandlerFailed,
                    "Handler threw an exception.",
                    operation.AuditPolicy);

                return Result(
                    RpcResultCode.HandlerFailed,
                    BuildErrorResponse(
                        envelope,
                        RpcResultCode.HandlerFailed,
                        "Majo operation failed safely."),
                    true,
                    false,
                    "Majo operation failed safely.");
            }

            if (handlerResult == null)
            {
                RecordAudit(
                    peer,
                    operation.OperationId,
                    RpcResultCode.HandlerFailed,
                    "Handler returned no result.",
                    operation.AuditPolicy);
                return Result(
                    RpcResultCode.HandlerFailed,
                    BuildErrorResponse(
                        envelope,
                        RpcResultCode.HandlerFailed,
                        "Majo operation failed safely."),
                    true,
                    false,
                    "Majo operation failed safely.");
            }

            if (handlerResult.Code == RpcResultCode.Success)
            {
                if (handlerResult.Payload.Length > MajoProtocol.MaxPayloadSize)
                {
                    _logger.Error(
                        "SecureRpc",
                        "Handler response exceeded protocol limit for operation " +
                        operation.OperationId + ".");
                    RecordAudit(
                        peer,
                        operation.OperationId,
                        RpcResultCode.HandlerFailed,
                        "Handler response exceeded protocol limit.",
                        operation.AuditPolicy);
                    return Result(
                        RpcResultCode.HandlerFailed,
                        BuildErrorResponse(
                            envelope,
                            RpcResultCode.HandlerFailed,
                            "Majo operation failed safely."),
                        true,
                        false,
                        "Majo operation failed safely.");
                }

                RecordAudit(
                    peer,
                    operation.OperationId,
                    RpcResultCode.Success,
                    "Operation completed.",
                    operation.AuditPolicy);

                var response = new MajoMessageEnvelope(
                    MajoMessageType.Response,
                    ProtocolVersion,
                    operation.OperationId,
                    envelope.RequestId,
                    new ArraySegment<byte>(handlerResult.Payload));

                return Result(
                    RpcResultCode.Success,
                    MajoMessageCodec.Encode(response),
                    true,
                    false,
                    string.Empty);
            }

            RecordAudit(
                peer,
                operation.OperationId,
                handlerResult.Code,
                "Handler returned controlled failure.",
                operation.AuditPolicy);

            return Result(
                handlerResult.Code,
                BuildErrorResponse(
                    envelope,
                    handlerResult.Code,
                    string.IsNullOrWhiteSpace(handlerResult.PublicMessage)
                        ? "Majo operation was rejected."
                        : handlerResult.PublicMessage),
                true,
                false,
                handlerResult.PublicMessage);
        }

        private GatewayDispatchResult HandleRemoteError(
            MajoPeerSession session,
            MajoMessageEnvelope envelope)
        {
            if (!MajoErrorCodec.TryDecode(
                envelope.Payload,
                out var code,
                out var publicMessage))
            {
                return Result(
                    RpcResultCode.InvalidPayload,
                    null,
                    false,
                    false,
                    "Invalid remote error message.");
            }

            if (code == RpcResultCode.UnsupportedProtocol)
            {
                session.RejectHandshake();
            }

            return Result(
                code,
                null,
                false,
                code == RpcResultCode.UnsupportedProtocol,
                publicMessage);
        }

        private GatewayDispatchResult RejectRequest(
            TrustedPeerContext peer,
            MajoMessageEnvelope envelope,
            RpcResultCode code,
            string auditReason,
            OperationDescriptor operation,
            bool disconnectPeer)
        {
            RecordAudit(
                peer,
                envelope.OperationId,
                code,
                auditReason,
                operation == null ? AuditPolicy.Failures : operation.AuditPolicy);

            return Result(
                code,
                BuildErrorResponse(
                    envelope,
                    code,
                    PublicReason(code)),
                false,
                disconnectPeer,
                PublicReason(code));
        }

        private byte[] BuildErrorResponse(
            MajoMessageEnvelope request,
            RpcResultCode code,
            string publicMessage)
        {
            if (request == null || request.RequestId <= 0)
            {
                return null;
            }

            var payload = MajoErrorCodec.Encode(code, publicMessage);
            var response = new MajoMessageEnvelope(
                MajoMessageType.Error,
                ProtocolVersion,
                request.OperationId,
                request.RequestId,
                new ArraySegment<byte>(payload));
            return MajoMessageCodec.Encode(response);
        }

        private bool HasRequiredCapabilities(MajoHelloPayload hello)
        {
            for (var index = 0; index < _requiredCapabilities.Count; index++)
            {
                if (!hello.HasCapability(_requiredCapabilities[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private void AuditInvalid(
            TrustedPeerContext peer,
            int operationId,
            RpcResultCode result,
            string reason)
        {
            if (!_rateLimiter.TryConsume(
                peer.ConnectionId,
                InvalidTrafficOperationId,
                InvalidTrafficPolicy))
            {
                return;
            }

            RecordAudit(
                peer,
                operationId,
                result,
                reason,
                AuditPolicy.Failures);
        }

        private void RecordAudit(
            TrustedPeerContext peer,
            int operationId,
            RpcResultCode result,
            string reason,
            AuditPolicy policy)
        {
            if (policy == AuditPolicy.None)
            {
                return;
            }

            if (policy == AuditPolicy.Failures && result == RpcResultCode.Success)
            {
                return;
            }

            _auditSink.Record(
                new AuditEvent(
                    DateTimeOffset.UtcNow,
                    peer.ConnectionId,
                    peer.PeerId,
                    peer.ActorId,
                    operationId,
                    result,
                    reason,
                    string.Empty));
        }

        private static bool DirectionMatchesLocalSide(
            RpcDirection direction,
            GatewayExecutionSide localSide)
        {
            if (localSide == GatewayExecutionSide.Server)
            {
                return direction == RpcDirection.ClientToServer;
            }

            return direction == RpcDirection.ServerToClient ||
                   direction == RpcDirection.ServerBroadcast;
        }

        private static string PublicReason(RpcResultCode code)
        {
            switch (code)
            {
                case RpcResultCode.Unauthorized:
                    return "Majo session is not authorized.";
                case RpcResultCode.Forbidden:
                    return "Majo operation is not permitted.";
                case RpcResultCode.InvalidPayload:
                    return "Majo request payload is invalid.";
                case RpcResultCode.UnsupportedProtocol:
                    return "Majo protocol is incompatible.";
                case RpcResultCode.RateLimited:
                    return "Majo request rate limit exceeded.";
                case RpcResultCode.DuplicateRequest:
                    return "Majo request was already processed.";
                case RpcResultCode.UnknownOperation:
                    return "Unknown Majo operation.";
                case RpcResultCode.HandlerFailed:
                    return "Majo operation failed safely.";
                case RpcResultCode.Unavailable:
                    return "Majo operation is unavailable.";
                default:
                    return string.Empty;
            }
        }

        private static GatewayDispatchResult Result(
            RpcResultCode code,
            byte[] responseBytes,
            bool handlerInvoked,
            bool disconnectPeer,
            string publicMessage)
        {
            return new GatewayDispatchResult(
                code,
                responseBytes,
                handlerInvoked,
                disconnectPeer,
                publicMessage);
        }

        private static void AddCapability(
            string capability,
            IList<string> capabilities,
            ISet<string> seen)
        {
            if (string.IsNullOrWhiteSpace(capability))
            {
                return;
            }

            var value = capability.Trim();
            if (value.Length > MajoProtocol.MaxCapabilityLength)
            {
                throw new ArgumentException("Capability exceeds protocol limit.");
            }

            if (seen.Add(value))
            {
                capabilities.Add(value);
            }
        }
    }
}
