using BepInEx;
using Jotunn.Utils;
using MajoServerModpack.Core.Runtime;

namespace MajoServerModpack.Platform.Framework
{
    internal static class FrameworkRuntimeMetadata
    {
        public static RuntimeMetadata Create()
        {
            return new RuntimeMetadata(
                MajoVersions.MajoVersion,
                MajoVersions.ProtocolVersion,
                MajoVersions.ConfigSchema,
                MajoVersions.DataSchema,
                GameVersions.ValheimVersion.ToString(),
                GetAssemblyVersion(typeof(BaseUnityPlugin)),
                GetAssemblyVersion(typeof(Jotunn.Main)));
        }

        private static string GetAssemblyVersion(System.Type type)
        {
            var version = type.Assembly.GetName().Version;
            return version == null ? "unknown" : version.ToString();
        }
    }
}
