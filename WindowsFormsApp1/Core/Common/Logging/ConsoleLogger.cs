using System;

namespace WindowsFormsApp1.Core.Common.Logging
{
    public class ConsoleLogger : ILogger
    {
        private readonly string _categoryName;
        
        public ConsoleLogger(string categoryName)
        {
            _categoryName = categoryName;
        }
        
        public void LogDebug(string message)
        {
            Console.WriteLine($"[DEBUG] [{_categoryName}] {message}");
        }
        
        public void LogInfo(string message)
        {
            Console.WriteLine($"[INFO] [{_categoryName}] {message}");
        }
        
        public void LogWarning(string message)
        {
            Console.WriteLine($"[WARN] [{_categoryName}] {message}");
        }
        
        public void LogError(string message)
        {
            Console.WriteLine($"[ERROR] [{_categoryName}] {message}");
        }
        
        public void LogError(Exception ex, string message)
        {
            Console.WriteLine($"[ERROR] [{_categoryName}] {message} | Exception: {ex.Message} | StackTrace: {ex.StackTrace}");
        }
        
        public void LogCritical(string message)
        {
            Console.WriteLine($"[CRITICAL] [{_categoryName}] {message}");
        }
        
        public void LogCritical(Exception ex, string message)
        {
            Console.WriteLine($"[CRITICAL] [{_categoryName}] {message} | Exception: {ex.Message} | StackTrace: {ex.StackTrace}");
        }
    }
}