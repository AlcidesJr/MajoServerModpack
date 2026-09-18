using System;

namespace MajoServerModpack.Core.Runtime
{
    public sealed class RuntimeMetadata
    {
        public RuntimeMetadata(
            string majoVersion,
            int protocolVersion,
            int configSchema,
            int dataSchema,
            string valheimVersion,
            string bepInExVersion,
            string jotunnVersion)
        {
            if (string.IsNullOrWhiteSpace(majoVersion))
            {
                throw new ArgumentException("Majo version is required.", nameof(majoVersion));
            }

            if (protocolVersion < 0 || configSchema < 0 || dataSchema < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(protocolVersion), "Schema/protocol versions cannot be negative.");
            }

            MajoVersion = majoVersion;
            ProtocolVersion = protocolVersion;
            ConfigSchema = configSchema;
            DataSchema = dataSchema;
            ValheimVersion = Normalize(valheimVersion);
            BepInExVersion = Normalize(bepInExVersion);
            JotunnVersion = Normalize(jotunnVersion);
        }

        public string MajoVersion { get; }
        public int ProtocolVersion { get; }
        public int ConfigSchema { get; }
        public int DataSchema { get; }
        public string ValheimVersion { get; }
        public string BepInExVersion { get; }
        public string JotunnVersion { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim();
        }
    }
}
