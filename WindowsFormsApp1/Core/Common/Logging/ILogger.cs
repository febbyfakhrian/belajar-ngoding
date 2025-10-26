using System;

namespace WindowsFormsApp1.Core.Common.Logging
{
    public interface ILogger
    {
        void LogDebug(string message);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogError(Exception ex, string message);
        void LogCritical(string message);
        void LogCritical(Exception ex, string message);
    }
}