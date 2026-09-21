using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MajoServerModpack.Core.Networking
{
    public static class MajoMessageCodec
    {
        public static byte[] Encode(MajoMessageEnvelope envelope)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }

            var payloadLength = envelope.Payload.Count;
            if (payloadLength < 0 || payloadLength > MajoProtocol.MaxPayloadSize)
            {
                throw new ArgumentOutOfRangeException(nameof(envelope), "Payload exceeds protocol limit.");
            }

            var totalLength = MajoProtocol.HeaderSize + payloadLength;
            if (totalLength > MajoProtocol.MaxEnvelopeSize)
            {
                throw new InvalidOperationException("Envelope exceeds protocol limit.");
            }

            var data = new byte[totalLength];
            var offset = 0;

            WriteInt32(data, ref offset, MajoProtocol.Magic);
            data[offset++] = MajoProtocol.WireFormatVersion;
            data[offset++] = (byte)envelope.MessageType;
            WriteInt32(data, ref offset, envelope.ProtocolVersion);
            WriteInt32(data, ref offset, envelope.OperationId);
            WriteInt64(data, ref offset, envelope.RequestId);
            WriteInt32(data, ref offset, payloadLength);

            if (payloadLength > 0)
            {
                Buffer.BlockCopy(
                    envelope.Payload.Array,
                    envelope.Payload.Offset,
                    data,
                    offset,
                    payloadLength);
            }

            return data;
        }

        public static bool TryDecode(
            byte[] data,
            out MajoMessageEnvelope envelope,
            out RpcResultCode errorCode,
            out string error)
        {
            envelope = null;
            errorCode = RpcResultCode.InvalidPayload;
            error = string.Empty;

            if (data == null)
            {
                error = "Envelope is null.";
                return false;
            }

            if (data.Length < MajoProtocol.HeaderSize)
            {
                error = "Envelope is truncated.";
                return false;
            }

            if (data.Length > MajoProtocol.MaxEnvelopeSize)
            {
                error = "Envelope exceeds maximum size.";
                return false;
            }

            try
            {
                var offset = 0;
                var magic = ReadInt32(data, ref offset);
                if (magic != MajoProtocol.Magic)
                {
                    error = "Envelope magic is invalid.";
                    return false;
                }

                var wireVersion = data[offset++];
                if (wireVersion != MajoProtocol.WireFormatVersion)
                {
                    errorCode = RpcResultCode.UnsupportedProtocol;
                    error = "Wire format version is unsupported.";
                    return false;
                }

                var rawMessageType = data[offset++];
                if (!TryParseMessageType(rawMessageType, out var messageType))
                {
                    error = "Message type is invalid.";
                    return false;
                }

                var protocolVersion = ReadInt32(data, ref offset);
                var operationId = ReadInt32(data, ref offset);
                var requestId = ReadInt64(data, ref offset);
                var payloadLength = ReadInt32(data, ref offset);

                if (payloadLength < 0 || payloadLength > MajoProtocol.MaxPayloadSize)
                {
                    error = "Payload length is invalid.";
                    return false;
                }

                if (payloadLength != data.Length - MajoProtocol.HeaderSize)
                {
                    error = "Payload length does not match envelope size.";
                    return false;
                }

                envelope = new MajoMessageEnvelope(
                    messageType,
                    protocolVersion,
                    operationId,
                    requestId,
                    new ArraySegment<byte>(data, MajoProtocol.HeaderSize, payloadLength));
                errorCode = RpcResultCode.Success;
                return true;
            }
            catch (IndexOutOfRangeException)
            {
                error = "Envelope is truncated.";
                return false;
            }
        }

        private static bool TryParseMessageType(byte raw, out MajoMessageType messageType)
        {
            messageType = (MajoMessageType)raw;
            return messageType == MajoMessageType.Hello ||
                   messageType == MajoMessageType.HelloAck ||
                   messageType == MajoMessageType.Request ||
                   messageType == MajoMessageType.Response ||
                   messageType == MajoMessageType.Error;
        }

        internal static void WriteInt32(byte[] data, ref int offset, int value)
        {
            data[offset++] = (byte)value;
            data[offset++] = (byte)(value >> 8);
            data[offset++] = (byte)(value >> 16);
            data[offset++] = (byte)(value >> 24);
        }

        internal static int ReadInt32(byte[] data, ref int offset)
        {
            var value =
                data[offset] |
                (data[offset + 1] << 8) |
                (data[offset + 2] << 16) |
                (data[offset + 3] << 24);
            offset += 4;
            return value;
        }

        internal static void WriteInt64(byte[] data, ref int offset, long value)
        {
            unchecked
            {
                data[offset++] = (byte)value;
                data[offset++] = (byte)(value >> 8);
                data[offset++] = (byte)(value >> 16);
                data[offset++] = (byte)(value >> 24);
                data[offset++] = (byte)(value >> 32);
                data[offset++] = (byte)(value >> 40);
                data[offset++] = (byte)(value >> 48);
                data[offset++] = (byte)(value >> 56);
            }
        }

        internal static long ReadInt64(byte[] data, ref int offset)
        {
            unchecked
            {
                var value =
                    (long)data[offset] |
                    ((long)data[offset + 1] << 8) |
                    ((long)data[offset + 2] << 16) |
                    ((long)data[offset + 3] << 24) |
                    ((long)data[offset + 4] << 32) |
                    ((long)data[offset + 5] << 40) |
                    ((long)data[offset + 6] << 48) |
                    ((long)data[offset + 7] << 56);
                offset += 8;
                return value;
            }
        }
    }

    public static class MajoHelloCodec
    {
        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        public static byte[] Encode(MajoHelloPayload hello)
        {
            if (hello == null)
            {
                throw new ArgumentNullException(nameof(hello));
            }

            var versionBytes = StrictUtf8.GetBytes(hello.MajoVersion);
            if (versionBytes.Length == 0 || versionBytes.Length > MajoProtocol.MaxVersionLength)
            {
                throw new InvalidOperationException("Majo version exceeds handshake limit.");
            }

            if (hello.Capabilities.Count > MajoProtocol.MaxHelloCapabilities)
            {
                throw new InvalidOperationException("Too many capabilities.");
            }

            var capabilityBytes = new List<byte[]>(hello.Capabilities.Count);
            var totalLength = 1 + 2 + versionBytes.Length + 1;

            for (var index = 0; index < hello.Capabilities.Count; index++)
            {
                var bytes = StrictUtf8.GetBytes(hello.Capabilities[index]);
                if (bytes.Length == 0 || bytes.Length > MajoProtocol.MaxCapabilityLength)
                {
                    throw new InvalidOperationException("Capability exceeds handshake limit.");
                }

                capabilityBytes.Add(bytes);
                totalLength += 2 + bytes.Length;
            }

            var data = new byte[totalLength];
            var offset = 0;
            data[offset++] = (byte)hello.ExecutionSide;
            WriteUInt16(data, ref offset, (ushort)versionBytes.Length);
            Buffer.BlockCopy(versionBytes, 0, data, offset, versionBytes.Length);
            offset += versionBytes.Length;
            data[offset++] = (byte)capabilityBytes.Count;

            for (var index = 0; index < capabilityBytes.Count; index++)
            {
                var bytes = capabilityBytes[index];
                WriteUInt16(data, ref offset, (ushort)bytes.Length);
                Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
                offset += bytes.Length;
            }

            return data;
        }

        public static bool TryDecode(
            ArraySegment<byte> payload,
            out MajoHelloPayload hello,
            out string error)
        {
            hello = null;
            error = string.Empty;

            if (payload.Array == null || payload.Count < 4)
            {
                error = "Hello payload is truncated.";
                return false;
            }

            var data = payload.Array;
            var offset = payload.Offset;
            var end = payload.Offset + payload.Count;

            var side = (GatewayExecutionSide)data[offset++];
            if (!MajoHelloPayload.IsValidExecutionSide(side))
            {
                error = "Hello execution side is invalid.";
                return false;
            }

            if (!TryReadString(
                data,
                ref offset,
                end,
                MajoProtocol.MaxVersionLength,
                out var version))
            {
                error = "Hello version is invalid.";
                return false;
            }

            if (offset >= end)
            {
                error = "Hello capability count is missing.";
                return false;
            }

            var count = data[offset++];
            if (count > MajoProtocol.MaxHelloCapabilities)
            {
                error = "Hello has too many capabilities.";
                return false;
            }

            var capabilities = new List<string>(count);
            var seen = new HashSet<string>(StringComparer.Ordinal);

            for (var index = 0; index < count; index++)
            {
                if (!TryReadString(
                    data,
                    ref offset,
                    end,
                    MajoProtocol.MaxCapabilityLength,
                    out var capability))
                {
                    error = "Hello capability is invalid.";
                    return false;
                }

                if (!seen.Add(capability))
                {
                    error = "Hello contains duplicate capability.";
                    return false;
                }

                capabilities.Add(capability);
            }

            if (offset != end)
            {
                error = "Hello payload has trailing data.";
                return false;
            }

            try
            {
                hello = new MajoHelloPayload(side, version, capabilities);
                return true;
            }
            catch (ArgumentException exception)
            {
                error = exception.Message;
                return false;
            }
        }

        private static bool TryReadString(
            byte[] data,
            ref int offset,
            int end,
            int maximumLength,
            out string value)
        {
            value = null;
            if (offset + 2 > end)
            {
                return false;
            }

            var length = ReadUInt16(data, ref offset);
            if (length == 0 || length > maximumLength || offset + length > end)
            {
                return false;
            }

            try
            {
                value = StrictUtf8.GetString(data, offset, length);
            }
            catch (DecoderFallbackException)
            {
                return false;
            }

            offset += length;
            return !string.IsNullOrWhiteSpace(value);
        }

        private static void WriteUInt16(byte[] data, ref int offset, ushort value)
        {
            data[offset++] = (byte)value;
            data[offset++] = (byte)(value >> 8);
        }

        private static ushort ReadUInt16(byte[] data, ref int offset)
        {
            var value = (ushort)(data[offset] | (data[offset + 1] << 8));
            offset += 2;
            return value;
        }
    }

    public static class MajoErrorCodec
    {
        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        public static byte[] Encode(RpcResultCode code, string publicMessage)
        {
            var message = publicMessage ?? string.Empty;
            if (message.Length > MajoProtocol.MaxPublicErrorLength)
            {
                message = message.Substring(0, MajoProtocol.MaxPublicErrorLength);
            }

            var bytes = StrictUtf8.GetBytes(message);
            if (bytes.Length > ushort.MaxValue)
            {
                throw new InvalidOperationException("Public error message is too large.");
            }

            var data = new byte[6 + bytes.Length];
            var offset = 0;
            MajoMessageCodec.WriteInt32(data, ref offset, (int)code);
            data[offset++] = (byte)bytes.Length;
            data[offset++] = (byte)(bytes.Length >> 8);

            if (bytes.Length > 0)
            {
                Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
            }

            return data;
        }

        public static bool TryDecode(
            ArraySegment<byte> payload,
            out RpcResultCode code,
            out string publicMessage)
        {
            code = RpcResultCode.InvalidPayload;
            publicMessage = string.Empty;

            if (payload.Array == null || payload.Count < 6)
            {
                return false;
            }

            var data = payload.Array;
            var offset = payload.Offset;
            var end = payload.Offset + payload.Count;
            var rawCode = MajoMessageCodec.ReadInt32(data, ref offset);
            if (!IsValidResultCode(rawCode))
            {
                return false;
            }

            var length = data[offset] | (data[offset + 1] << 8);
            offset += 2;

            if (length < 0 ||
                length > MajoProtocol.MaxPublicErrorLength * 4 ||
                offset + length != end)
            {
                return false;
            }

            try
            {
                publicMessage = length == 0
                    ? string.Empty
                    : StrictUtf8.GetString(data, offset, length);
            }
            catch (DecoderFallbackException)
            {
                return false;
            }

            if (publicMessage.Length > MajoProtocol.MaxPublicErrorLength)
            {
                return false;
            }

            code = (RpcResultCode)rawCode;
            return true;
        }

        private static bool IsValidResultCode(int code)
        {
            return code >= (int)RpcResultCode.Success &&
                   code <= (int)RpcResultCode.Unavailable;
        }
    }
}
