using System;
using BepInEx.Logging;
using MajoServerModpack.Core.Logging;

namespace MajoServerModpack.Platform.Framework
{
    internal sealed class BepInExMajoLogger : IMajoLogger
    {
        private readonly ManualLogSource _source;

        public BepInExMajoLogger(ManualLogSource source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public void Debug(string category, string message)
        {
            _source.LogDebug(Format(category, message));
        }

        public void Info(string category, string message)
        {
            _source.LogInfo(Format(category, message));
        }

        public void Warning(string category, string message)
        {
            _source.LogWarning(Format(category, message));
        }

        public void Error(string category, string message, Exception exception = null)
        {
            _source.LogError(Format(category, message));
            if (exception != null)
            {
                _source.LogError(exception);
            }
        }

        private static string Format(string category, string message)
        {
            var safeCategory = string.IsNullOrWhiteSpace(category) ? "Core" : category.Trim();
            return "[" + safeCategory + "] " + (message ?? string.Empty);
        }
    }
}
