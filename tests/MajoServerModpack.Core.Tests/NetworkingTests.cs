using System;
using System.Collections.Generic;
using System.Text;
using MajoServerModpack.Core.Logging;
using MajoServerModpack.Core.Networking;

internal static class NetworkingTests
{
    private static int _failed;

    public static int RunAll()
    {
        _failed = 0;

        Run("RPC operation registry", TestOperationRegistry);
        Run("RPC invalid direction metadata", TestInvalidDirection);
        Run("RPC session handshake", TestSessionHandshake);
        Run("RPC duplicate handshake", TestDuplicateHandshake);
        Run("RPC disconnect reconnect cleanup", TestDisconnectReconnect);
        Run("RPC authorization levels", TestAuthorizationLevels);
        Run("RPC unknown peer", TestUnknownPeer);
        Run("RPC codec roundtrip", TestCodecRoundtrip);
        Run("RPC malformed envelope", TestMalformedEnvelope);
        Run("RPC oversized envelope", TestOversizedEnvelope);
        Run("RPC invalid protocol", TestInvalidProtocol);
        Run("RPC missing capability", TestMissingCapability);
        Run("RPC valid dispatch", TestValidDispatch);
        Run("RPC spoofed actor ignored", TestSpoofedActor);
        Run("RPC spoofed admin ignored", TestSpoofedAdmin);
        Run("RPC semantic validation", TestSemanticValidation);
        Run("RPC finite/range helpers", TestPayloadRules);
        Run("RPC rate limit burst and expiry", TestRateLimitBurstAndExpiry);
        Run("RPC independent peers", TestIndependentPeers);
        Run("RPC independent operations", TestIndependentOperations);
        Run("RPC global peer quota", TestGlobalPeerQuota);
        Run("RPC replayed request", TestReplay);
        Run("RPC handler failure isolation", TestHandlerFailureIsolation);
        Run("RPC unknown operation", TestUnknownOperation);
        Run("RPC invalid traffic audit quota", TestInvalidTrafficAuditQuota);
        Run("RPC client hello acknowledgement", TestClientHelloAcknowledgement);

        return _failed;
    }

    private static void Run(string name, Action test)
    {
        try
        {
            test();
            Console.WriteLine("[PASS] " + name);
        }
        catch (Exception exception)
        {
            _failed++;
            Console.WriteLine("[FAIL] " + name + ": " + exception.Message);
        }
    }

    private static void TestOperationRegistry()
    {
        var registry = new OperationRegistry();
        var operation = Operation(10, MajoPermissions.Admin);
        registry.Register(operation);

        Assert(registry.Count == 1, "valid operation must register");
        Assert(registry.TryGet(10, out var found), "registered operation must be found");
        Assert(found.RequiredPermission == MajoPermissions.Admin, "permission metadata must be preserved");
        Assert(found.Handler != null, "handler lookup must be preserved");
        AssertThrows<InvalidOperationException>(() => registry.Register(operation));
    }

    private static void TestInvalidDirection()
    {
        AssertThrows<ArgumentOutOfRangeException>(() =>
            new OperationDescriptor(
                10,
                (RpcDirection)999,
                GatewayExecutionSide.Server,
                MajoPermissions.Player,
                16,
                new RateLimitPolicy(5, 0, TimeSpan.FromSeconds(1)),
                AuditPolicy.Failures,
                1,
                payload => PayloadValidationResult.Valid(),
                (context, payload) => RpcHandlerResult.Success()));
    }

    private static void TestSessionHandshake()
    {
        var gateway = Gateway();
        var peer = Peer("session-a", 101);

        gateway.Connect(peer);
        var result = HandshakeServer(gateway, peer, 1);

        Assert(result.Code == RpcResultCode.Success, "handshake must succeed");
        Assert(result.ShouldRespond, "server must answer hello");
        Assert(gateway.IsCompatible(peer.ConnectionId), "session must become compatible");
    }

    private static void TestDuplicateHandshake()
    {
        var gateway = Gateway();
        var peer = Peer("session-duplicate", 102);
        gateway.Connect(peer);

        Assert(HandshakeServer(gateway, peer, 1).Code == RpcResultCode.Success, "first hello must pass");
        var duplicate = HandshakeServer(gateway, peer, 2);

        Assert(duplicate.Code == RpcResultCode.DuplicateRequest, "duplicate hello must be rejected");
        Assert(duplicate.DisconnectPeer, "duplicate handshake must fail closed");
    }

    private static void TestDisconnectReconnect()
    {
        var clock = new FakeClock();
        var gateway = Gateway(clock);
        var peer = Peer("session-old", 103);
        gateway.Connect(peer);
        HandshakeServer(gateway, peer, 1);

        gateway.Register(Operation(20, MajoPermissions.Player));
        var first = gateway.ProcessIncoming(
            peer,
            GatewayExecutionSide.Server,
            gateway.CreateRequest(20, 10, new byte[] { 1 }));
        Assert(first.Code == RpcResultCode.Success, "first connection must dispatch");

        Assert(gateway.Disconnect(peer.ConnectionId), "disconnect must remove session");
        Assert(gateway.Sessions.Count == 0, "session registry must be empty after disconnect");

        var reconnected = Peer("session-new", 103);
        gateway.Connect(reconnected);
        Assert(HandshakeServer(gateway, reconnected, 2).Code == RpcResultCode.Success, "reconnect must handshake");
        Assert(gateway.IsCompatible(reconnected.ConnectionId), "new connection must be independent");
    }

    private static void TestAuthorizationLevels()
    {
        var authorizer = new PermissionAuthorizer();
        var player = Peer("auth-player", 1);
        var admin = Peer("auth-admin", 2, isAdmin: true);
        var system = Peer("auth-system", 0, isSystem: true);
        var custom = Peer("auth-custom", 3, grants: new[] { "majo.config.write" });

        Assert(authorizer.Authorize(player, MajoPermissions.Player) == RpcResultCode.Success, "player permission");
        Assert(authorizer.Authorize(player, MajoPermissions.Admin) == RpcResultCode.Forbidden, "player is not admin");
        Assert(authorizer.Authorize(admin, MajoPermissions.Admin) == RpcResultCode.Success, "admin permission");
        Assert(authorizer.Authorize(admin, MajoPermissions.System) == RpcResultCode.Forbidden, "admin is not system");
        Assert(authorizer.Authorize(system, MajoPermissions.System) == RpcResultCode.Success, "system permission");
        Assert(authorizer.Authorize(custom, "majo.config.write") == RpcResultCode.Success, "future custom grant");
        Assert(authorizer.Authorize(null, MajoPermissions.Player) == RpcResultCode.Unauthorized, "null peer unauthorized");
    }

    private static void TestUnknownPeer()
    {
        var gateway = Gateway();
        gateway.Register(Operation(30, MajoPermissions.Player));
        var unknown = Peer("unknown", 999);

        var result = gateway.ProcessIncoming(
            unknown,
            GatewayExecutionSide.Server,
            gateway.CreateRequest(30, 1, Array.Empty<byte>()));

        Assert(result.Code == RpcResultCode.Unauthorized, "unknown connection must be rejected");
        Assert(!result.HandlerInvoked, "unknown connection must not invoke handler");
    }

    private static void TestCodecRoundtrip()
    {
        var payload = new byte[] { 1, 2, 3, 4 };
        var original = new MajoMessageEnvelope(
            MajoMessageType.Request,
            1,
            55,
            123456789,
            new ArraySegment<byte>(payload));

        var bytes = MajoMessageCodec.Encode(original);
        Assert(MajoMessageCodec.TryDecode(bytes, out var decoded, out var code, out var error),
            "roundtrip decode failed: " + code + " " + error);
        Assert(decoded.ProtocolVersion == 1, "protocol roundtrip");
        Assert(decoded.OperationId == 55, "operation roundtrip");
        Assert(decoded.RequestId == 123456789, "request id roundtrip");
        Assert(decoded.Payload.Count == 4, "payload roundtrip");
    }

    private static void TestMalformedEnvelope()
    {
        var gateway = Gateway();
        var peer = Peer("malformed", 1);
        gateway.Connect(peer);

        var result = gateway.ProcessIncoming(
            peer,
            GatewayExecutionSide.Server,
            new byte[] { 1, 2, 3 });

        Assert(result.Code == RpcResultCode.InvalidPayload, "truncated packet must be invalid");
        Assert(!result.HandlerInvoked, "malformed packet must not reach handler");
    }

    private static void TestOversizedEnvelope()
    {
        var data = new byte[MajoProtocol.MaxEnvelopeSize + 1];
        Assert(!MajoMessageCodec.TryDecode(data, out _, out var code, out _),
            "oversized packet must not decode");
        Assert(code == RpcResultCode.InvalidPayload, "oversized packet error code");
    }

    private static void TestInvalidProtocol()
    {
        var gateway = Gateway();
        var peer = Peer("protocol", 1);
        gateway.Connect(peer);

        var hello = new MajoHelloPayload(
            GatewayExecutionSide.Client,
            "0.0.2",
            new[] { MajoProtocol.CoreNetworkCapability });
        var envelope = new MajoMessageEnvelope(
            MajoMessageType.Hello,
            999,
            MajoProtocol.HandshakeOperationId,
            1,
            new ArraySegment<byte>(MajoHelloCodec.Encode(hello)));

        var result = gateway.ProcessIncoming(
            peer,
            GatewayExecutionSide.Server,
            MajoMessageCodec.Encode(envelope));

        Assert(result.Code == RpcResultCode.UnsupportedProtocol, "unsupported protocol must reject");
        Assert(result.DisconnectPeer, "protocol mismatch must fail closed");
        Assert(!gateway.IsCompatible(peer.ConnectionId), "incompatible session must not become ready");
    }

    private static void TestMissingCapability()
    {
        var gateway = Gateway();
        var peer = Peer("missing-cap", 1);
        gateway.Connect(peer);

        var hello = new MajoHelloPayload(
            GatewayExecutionSide.Client,
            "0.0.2",
            Array.Empty<string>());
        var envelope = new MajoMessageEnvelope(
            MajoMessageType.Hello,
            1,
            0,
            1,
            new ArraySegment<byte>(MajoHelloCodec.Encode(hello)));

        var result = gateway.ProcessIncoming(
            peer,
            GatewayExecutionSide.Server,
            MajoMessageCodec.Encode(envelope));

        Assert(result.Code == RpcResultCode.UnsupportedProtocol, "missing capability must reject");
        Assert(result.DisconnectPeer, "missing capability must fail closed");
    }

    private static void TestValidDispatch()
    {
        var gateway = Gateway();
        var peer = ReadyPeer(gateway, "dispatch", 1);
        var invoked = false;

        gateway.Register(
            Operation(
                40,
                MajoPermissions.Player,
                handler: (context, payload) =>
                {
                    invoked = true;
                    return RpcHandlerResult.Success(new byte[] { 9 });
                }));

        var result = gateway.ProcessIncoming(
            peer,
            GatewayExecutionSide.Server,
            gateway.CreateRequest(40, 10, new byte[] { 1 }));

        Assert(result.Code == RpcResultCode.Success, "valid request must pass");
        Assert(result.HandlerInvoked && invoked, "handler must run");
        Assert(result.ShouldRespond, "valid request must receive response");
        Assert(MajoMessageCodec.TryDecode(result.ResponseBytes, out var response, out _, out _),
            "response must decode");
        Assert(response.MessageType == MajoMessageType.Response, "response type");
    }

    private static void TestSpoofedActor()
    {
        var gateway = Gateway();
        var peer = ReadyPeer(gateway, "actor", 1, actorId: "trusted-actor");
        string observedActor = null;

        gateway.Register(
            Operation(
                41,
                MajoPermissions.Player,
                handler: (context, payload) =>
                {
                    observedActor = context.Peer.ActorId;
                    return RpcHandlerResult.Success();
                }));

        var spoofed = Encoding.UTF8.GetBytes("attacker-declared-actor");
        var result = gateway.ProcessIncoming(
            peer,
            GatewayExecutionSide.Server,
            gateway.CreateRequest(41, 11, spoofed));

        Assert(result.Code == RpcResultCode.Success, "player operation should run");
        Assert(observedActor == "trusted-actor", "payload must not replace trusted actor");
    }

    private static void TestSpoofedAdmin()
    {
        var gateway = Gateway();
        var peer = ReadyPeer(gateway, "admin-spoof", 1, isAdmin: false);
        var invoked = false;

        gateway.Register(
            Operation(
                42,
                MajoPermissions.Admin,
                handler: (context, payload) =>
                {
                    invoked = true;
                    return RpcHandlerResult.Success();
                }));

        var result = gateway.ProcessIncoming(
            peer,
            GatewayExecutionSide.Server,
            gateway.CreateRequest(42, 12, new byte[] { 1 }));

        Assert(result.Code == RpcResultCode.Forbidden, "client admin flag must not grant admin");
        Assert(!invoked && !result.HandlerInvoked, "forbidden handler must not run");
    }

    private static void TestSemanticValidation()
    {
        var gateway = Gateway();
        var peer = ReadyPeer(gateway, "validation", 1);
        var invoked = false;

        gateway.Register(
            Operation(
                43,
                MajoPermissions.Player,
                validator: payload =>
                {
                    if (payload.Count != 1 || payload.Array[payload.Offset] > 2)
                    {
                        return PayloadValidationResult.Invalid("enum/range invalid");
                    }

                    return PayloadValidationResult.Valid();
                },
                handler: (context, payload) =>
                {
                    invoked = true;
                    return RpcHandlerResult.Success();
                }));

        var result = gateway.ProcessIncoming(
            peer,
            GatewayExecutionSide.Server,
            gateway.CreateRequest(43, 13, new byte[] { 9 }));

        Assert(result.Code == RpcResultCode.InvalidPayload, "invalid enum/range must reject");
        Assert(!invoked, "invalid payload must not reach handler");
    }

    private static void TestPayloadRules()
    {
        Assert(!PayloadRules.IsFinite(float.NaN), "NaN invalid");
        Assert(!PayloadRules.IsFinite(float.PositiveInfinity), "Infinity invalid");
        Assert(PayloadRules.IsFinite(10.5f), "finite value valid");
        Assert(PayloadRules.IsCountWithin(10, 10), "max collection count valid");
        Assert(!PayloadRules.IsCountWithin(11, 10), "oversized collection invalid");
        Assert(!PayloadRules.IsStringWithin("abcd", 3), "oversized string invalid");
    }

    private static void TestRateLimitBurstAndExpiry()
    {
        var clock = new FakeClock();
        var gateway = Gateway(clock);
        var peer = ReadyPeer(gateway, "rate", 1);

        gateway.Register(
            Operation(
                50,
                MajoPermissions.Player,
                policy: new RateLimitPolicy(2, 1, TimeSpan.FromSeconds(1))));

        Assert(Request(gateway, peer, 50, 1).Code == RpcResultCode.Success, "request 1");
        Assert(Request(gateway, peer, 50, 2).Code == RpcResultCode.Success, "request 2");
        Assert(Request(gateway, peer, 50, 3).Code == RpcResultCode.Success, "burst request");
        Assert(Request(gateway, peer, 50, 4).Code == RpcResultCode.RateLimited, "limit must trigger");

        clock.Advance(TimeSpan.FromSeconds(2));
        Assert(Request(gateway, peer, 50, 5).Code == RpcResultCode.Success, "window must reset");
    }

    private static void TestIndependentPeers()
    {
        var clock = new FakeClock();
        var gateway = Gateway(clock);
        var a = ReadyPeer(gateway, "peer-a", 1);
        var b = ReadyPeer(gateway, "peer-b", 2);
        gateway.Register(
            Operation(
                51,
                MajoPermissions.Player,
                policy: new RateLimitPolicy(1, 0, TimeSpan.FromSeconds(10))));

        Assert(Request(gateway, a, 51, 1).Code == RpcResultCode.Success, "peer A first");
        Assert(Request(gateway, a, 51, 2).Code == RpcResultCode.RateLimited, "peer A limited");
        Assert(Request(gateway, b, 51, 1).Code == RpcResultCode.Success, "peer B independent");
    }

    private static void TestIndependentOperations()
    {
        var clock = new FakeClock();
        var gateway = Gateway(clock);
        var peer = ReadyPeer(gateway, "op-independent", 1);
        var policy = new RateLimitPolicy(1, 0, TimeSpan.FromSeconds(10));
        gateway.Register(Operation(52, MajoPermissions.Player, policy: policy));
        gateway.Register(Operation(53, MajoPermissions.Player, policy: policy));

        Assert(Request(gateway, peer, 52, 1).Code == RpcResultCode.Success, "op52 first");
        Assert(Request(gateway, peer, 52, 2).Code == RpcResultCode.RateLimited, "op52 limited");
        Assert(Request(gateway, peer, 53, 3).Code == RpcResultCode.Success, "op53 independent");
    }

    private static void TestGlobalPeerQuota()
    {
        var clock = new FakeClock();
        var gateway = Gateway(clock);
        var peer = ReadyPeer(gateway, "global-rate", 1);
        var generous = new RateLimitPolicy(1000, 0, TimeSpan.FromSeconds(10));
        gateway.Register(Operation(54, MajoPermissions.Player, policy: generous));
        gateway.Register(Operation(55, MajoPermissions.Player, policy: generous));

        RpcResultCode last = RpcResultCode.Success;
        for (var index = 1; index <= 81; index++)
        {
            var operation = index % 2 == 0 ? 54 : 55;
            last = Request(gateway, peer, operation, index).Code;
        }

        Assert(last == RpcResultCode.RateLimited,
            "rotating operation IDs must not bypass global peer quota");
    }

    private static void TestReplay()
    {
        var gateway = Gateway();
        var peer = ReadyPeer(gateway, "replay", 1);
        var calls = 0;
        gateway.Register(
            Operation(
                56,
                MajoPermissions.Player,
                handler: (context, payload) =>
                {
                    calls++;
                    return RpcHandlerResult.Success();
                }));

        var bytes = gateway.CreateRequest(56, 99, Array.Empty<byte>());
        Assert(gateway.ProcessIncoming(peer, GatewayExecutionSide.Server, bytes).Code == RpcResultCode.Success,
            "first request must pass");
        var replay = gateway.ProcessIncoming(peer, GatewayExecutionSide.Server, bytes);

        Assert(replay.Code == RpcResultCode.DuplicateRequest, "replayed request must reject");
        Assert(calls == 1, "replay must not invoke handler again");
    }

    private static void TestHandlerFailureIsolation()
    {
        var logger = new MemoryLogger();
        var gateway = Gateway(logger: logger);
        var peer = ReadyPeer(gateway, "handler-fail", 1);
        gateway.Register(
            Operation(
                57,
                MajoPermissions.Player,
                handler: (context, payload) =>
                    throw new InvalidOperationException("secret-stack-detail")));

        var result = Request(gateway, peer, 57, 1);
        Assert(result.Code == RpcResultCode.HandlerFailed, "handler exception must be isolated");
        Assert(result.ShouldRespond, "handler exception returns safe error");
        Assert(logger.ErrorCount == 1, "internal exception must be logged");

        Assert(MajoMessageCodec.TryDecode(result.ResponseBytes, out var envelope, out _, out _),
            "safe error response must decode");
        Assert(MajoErrorCodec.TryDecode(envelope.Payload, out var code, out var message),
            "safe error payload must decode");
        Assert(code == RpcResultCode.HandlerFailed, "safe error code");
        Assert(!message.Contains("secret-stack-detail"), "client error must not leak exception detail");
    }

    private static void TestUnknownOperation()
    {
        var gateway = Gateway();
        var peer = ReadyPeer(gateway, "unknown-op", 1);
        var envelope = new MajoMessageEnvelope(
            MajoMessageType.Request,
            1,
            9999,
            1,
            new ArraySegment<byte>(Array.Empty<byte>()));

        var result = gateway.ProcessIncoming(
            peer,
            GatewayExecutionSide.Server,
            MajoMessageCodec.Encode(envelope));

        Assert(result.Code == RpcResultCode.UnknownOperation, "unknown operation must reject safely");
        Assert(!result.HandlerInvoked, "unknown operation must not invoke handler");
    }

    private static void TestInvalidTrafficAuditQuota()
    {
        var audit = new MemoryAuditSink();
        var gateway = Gateway(audit: audit);
        var peer = Peer("invalid-flood", 1);
        gateway.Connect(peer);

        for (var index = 0; index < 20; index++)
        {
            gateway.ProcessIncoming(
                peer,
                GatewayExecutionSide.Server,
                new byte[] { 1, 2, 3 });
        }

        Assert(audit.Count == 5, "invalid traffic audit must be rate-limited");
    }

    private static void TestClientHelloAcknowledgement()
    {
        var server = Gateway();
        var client = Gateway();
        var serverViewOfClient = Peer("server-side", 11);
        var clientViewOfServer = Peer("client-side", 22, isServerPeer: true);

        server.Connect(serverViewOfClient);
        client.Connect(clientViewOfServer);

        var hello = client.CreateHello(GatewayExecutionSide.Client, 7);
        var serverResult = server.ProcessIncoming(
            serverViewOfClient,
            GatewayExecutionSide.Server,
            hello);

        Assert(serverResult.Code == RpcResultCode.Success && serverResult.ShouldRespond,
            "server must answer compatible hello");

        var clientResult = client.ProcessIncoming(
            clientViewOfServer,
            GatewayExecutionSide.Client,
            serverResult.ResponseBytes);

        Assert(clientResult.Code == RpcResultCode.Success, "client must accept hello ack");
        Assert(client.IsCompatible(clientViewOfServer.ConnectionId), "client session must become compatible");
    }

    private static GatewayDispatchResult Request(
        SecureRpcGateway gateway,
        TrustedPeerContext peer,
        int operationId,
        long requestId)
    {
        return gateway.ProcessIncoming(
            peer,
            GatewayExecutionSide.Server,
            gateway.CreateRequest(operationId, requestId, Array.Empty<byte>()));
    }

    private static GatewayDispatchResult HandshakeServer(
        SecureRpcGateway gateway,
        TrustedPeerContext peer,
        long requestId)
    {
        var hello = new MajoHelloPayload(
            GatewayExecutionSide.Client,
            "0.0.2",
            new[] { MajoProtocol.CoreNetworkCapability });
        var envelope = new MajoMessageEnvelope(
            MajoMessageType.Hello,
            1,
            MajoProtocol.HandshakeOperationId,
            requestId,
            new ArraySegment<byte>(MajoHelloCodec.Encode(hello)));

        return gateway.ProcessIncoming(
            peer,
            GatewayExecutionSide.Server,
            MajoMessageCodec.Encode(envelope));
    }

    private static TrustedPeerContext ReadyPeer(
        SecureRpcGateway gateway,
        string connectionId,
        long peerId,
        string actorId = null,
        bool isAdmin = false)
    {
        var peer = Peer(
            connectionId,
            peerId,
            actorId,
            isAdmin);
        gateway.Connect(peer);
        var result = HandshakeServer(gateway, peer, peerId + 1000);
        Assert(result.Code == RpcResultCode.Success, "test peer handshake failed");
        return peer;
    }

    private static OperationDescriptor Operation(
        int id,
        string permission,
        RateLimitPolicy policy = null,
        RpcPayloadValidator validator = null,
        RpcOperationHandler handler = null)
    {
        return new OperationDescriptor(
            id,
            RpcDirection.ClientToServer,
            GatewayExecutionSide.Server,
            permission,
            1024,
            policy ?? new RateLimitPolicy(1000, 0, TimeSpan.FromSeconds(10)),
            AuditPolicy.Failures,
            1,
            validator ?? (payload => PayloadValidationResult.Valid()),
            handler ?? ((context, payload) => RpcHandlerResult.Success()));
    }

    private static TrustedPeerContext Peer(
        string connectionId,
        long peerId,
        string actorId = null,
        bool isAdmin = false,
        bool isSystem = false,
        bool isServerPeer = false,
        IEnumerable<string> grants = null)
    {
        return new TrustedPeerContext(
            connectionId,
            peerId,
            actorId ?? ("peer:" + peerId),
            isAdmin,
            isSystem,
            isServerPeer,
            grants);
    }

    private static SecureRpcGateway Gateway(
        FakeClock clock = null,
        MemoryAuditSink audit = null,
        MemoryLogger logger = null)
    {
        var effectiveLogger = logger ?? new MemoryLogger();
        return new SecureRpcGateway(
            effectiveLogger,
            "0.0.2",
            1,
            new[] { MajoProtocol.CoreNetworkCapability },
            clock ?? new FakeClock(),
            audit ?? new MemoryAuditSink());
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertThrows<T>(Action action) where T : Exception
    {
        try
        {
            action();
        }
        catch (T)
        {
            return;
        }

        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }

    private sealed class FakeClock : IRateLimitClock
    {
        public long NowTicks { get; private set; }

        public void Advance(TimeSpan duration)
        {
            NowTicks += duration.Ticks;
        }
    }

    private sealed class MemoryAuditSink : IAuditSink
    {
        public int Count { get; private set; }

        public void Record(AuditEvent auditEvent)
        {
            Count++;
        }
    }

    private sealed class MemoryLogger : IMajoLogger
    {
        public int ErrorCount { get; private set; }

        public void Debug(string category, string message) { }
        public void Info(string category, string message) { }
        public void Warning(string category, string message) { }

        public void Error(string category, string message, Exception exception = null)
        {
            ErrorCount++;
        }
    }
}
