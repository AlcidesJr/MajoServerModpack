using System;

namespace MajoServerModpack.Core.Logging
{
    public interface IMajoLogger
    {
        void Debug(string category, string message);
        void Info(string category, string message);
        void Warning(string category, string message);
        void Error(string category, string message, Exception exception = null);
    }
}
