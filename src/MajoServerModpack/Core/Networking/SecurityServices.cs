using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MajoServerModpack.Core.Logging;

namespace MajoServerModpack.Core.Networking
{
    public interface IRateLimitClock
    {
        long NowTicks { get; }
    }

    public sealed class SystemRateLimitClock : IRateLimitClock
    {
        public long NowTicks
        {
            get
            {
                var timestamp = Stopwatch.GetTimestamp();
                return (long)(timestamp * (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency);
            }
        }
    }

    public sealed class FixedWindowRateLimiter
    {
        private struct RateLimitKey : IEquatable<RateLimitKey>
        {
            public RateLimitKey(string connectionId, int operationId)
            {
                ConnectionId = connectionId;
                OperationId = operationId;
            }

            public string ConnectionId { get; }
            public int OperationId { get; }

            public bool Equals(RateLimitKey other)
            {
                return OperationId == other.OperationId &&
                       string.Equals(ConnectionId, other.ConnectionId, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                return obj is RateLimitKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((ConnectionId != null ? StringComparer.Ordinal.GetHashCode(ConnectionId) : 0) * 397) ^
                           OperationId;
                }
            }
        }

        private sealed class WindowState
        {
            public long StartedAt;
            public int Count;
        }

        private readonly IRateLimitClock _clock;
        private readonly Dictionary<RateLimitKey, WindowState> _states =
            new Dictionary<RateLimitKey, WindowState>();

        public FixedWindowRateLimiter(IRateLimitClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public int StateCount => _states.Count;

        public bool TryConsume(
            string connectionId,
            int operationId,
            RateLimitPolicy policy)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                return false;
            }

            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            var now = _clock.NowTicks;
            var key = new RateLimitKey(connectionId, operationId);

            if (!_states.TryGetValue(key, out var state))
            {
                state = new WindowState
                {
                    StartedAt = now,
                    Count = 1
                };
                _states.Add(key, state);
                return true;
            }

            var elapsed = now - state.StartedAt;
            if (elapsed < 0 || elapsed >= policy.Window.Ticks)
            {
                state.StartedAt = now;
                state.Count = 1;
                return true;
            }

            if (state.Count >= policy.Capacity)
            {
                return false;
            }

            state.Count++;
            return true;
        }

        public void ClearConnection(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId) || _states.Count == 0)
            {
                return;
            }

            var remove = new List<RateLimitKey>();
            foreach (var pair in _states)
            {
                if (string.Equals(pair.Key.ConnectionId, connectionId, StringComparison.Ordinal))
                {
                    remove.Add(pair.Key);
                }
            }

            for (var index = 0; index < remove.Count; index++)
            {
                _states.Remove(remove[index]);
            }
        }

        public void Clear()
        {
            _states.Clear();
        }
    }

    public sealed class PermissionAuthorizer
    {
        public RpcResultCode Authorize(TrustedPeerContext peer, string requiredPermission)
        {
            if (peer == null)
            {
                return RpcResultCode.Unauthorized;
            }

            if (string.IsNullOrWhiteSpace(requiredPermission))
            {
                return RpcResultCode.Forbidden;
            }

            var permission = requiredPermission.Trim();

            if (string.Equals(permission, MajoPermissions.None, StringComparison.Ordinal))
            {
                return RpcResultCode.Success;
            }

            if (string.Equals(permission, MajoPermissions.Player, StringComparison.Ordinal))
            {
                return RpcResultCode.Success;
            }

            if (string.Equals(permission, MajoPermissions.Admin, StringComparison.Ordinal))
            {
                return peer.IsSystem || peer.IsAdmin
                    ? RpcResultCode.Success
                    : RpcResultCode.Forbidden;
            }

            if (string.Equals(permission, MajoPermissions.System, StringComparison.Ordinal))
            {
                return peer.IsSystem
                    ? RpcResultCode.Success
                    : RpcResultCode.Forbidden;
            }

            return peer.IsSystem || peer.HasGrantedPermission(permission)
                ? RpcResultCode.Success
                : RpcResultCode.Forbidden;
        }
    }

    public sealed class AuditEvent
    {
        public AuditEvent(
            DateTimeOffset timestamp,
            string connectionId,
            long peerId,
            string actorId,
            int operationId,
            RpcResultCode result,
            string reason,
            string target)
        {
            Timestamp = timestamp;
            ConnectionId = connectionId ?? string.Empty;
            PeerId = peerId;
            ActorId = actorId ?? string.Empty;
            OperationId = operationId;
            Result = result;
            Reason = reason ?? string.Empty;
            Target = target ?? string.Empty;
        }

        public DateTimeOffset Timestamp { get; }
        public string ConnectionId { get; }
        public long PeerId { get; }
        public string ActorId { get; }
        public int OperationId { get; }
        public RpcResultCode Result { get; }
        public string Reason { get; }
        public string Target { get; }
    }

    public interface IAuditSink
    {
        void Record(AuditEvent auditEvent);
    }

    public sealed class LoggerAuditSink : IAuditSink
    {
        private readonly IMajoLogger _logger;

        public LoggerAuditSink(IMajoLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Record(AuditEvent auditEvent)
        {
            if (auditEvent == null)
            {
                throw new ArgumentNullException(nameof(auditEvent));
            }

            var message = new StringBuilder(192);
            message.Append("peer=").Append(auditEvent.PeerId);
            message.Append(" connection=").Append(Sanitize(auditEvent.ConnectionId, 48));
            message.Append(" actor=").Append(Sanitize(auditEvent.ActorId, 64));
            message.Append(" operation=").Append(auditEvent.OperationId);
            message.Append(" result=").Append(auditEvent.Result);

            if (!string.IsNullOrEmpty(auditEvent.Reason))
            {
                message.Append(" reason=").Append(Sanitize(auditEvent.Reason, 96));
            }

            if (!string.IsNullOrEmpty(auditEvent.Target))
            {
                message.Append(" target=").Append(Sanitize(auditEvent.Target, 64));
            }

            if (auditEvent.Result == RpcResultCode.Success)
            {
                _logger.Info("SecureRpcAudit", message.ToString());
            }
            else
            {
                _logger.Warning("SecureRpcAudit", message.ToString());
            }
        }

        private static string Sanitize(string value, int maximumLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "-";
            }

            var length = value.Length > maximumLength ? maximumLength : value.Length;
            var chars = new char[length];

            for (var index = 0; index < length; index++)
            {
                var current = value[index];
                chars[index] = char.IsControl(current) ? '?' : current;
            }

            return new string(chars);
        }
    }

    public sealed class NullAuditSink : IAuditSink
    {
        public void Record(AuditEvent auditEvent)
        {
        }
    }
}
