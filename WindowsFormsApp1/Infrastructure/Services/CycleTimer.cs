using System;
using System.Diagnostics;

namespace WindowsFormsApp1.Infrastructure.Services.Services
{
    /// <summary>
    /// A simple timer utility for measuring cycle times in operations.
    /// </summary>
    public static class CycleTimer
    {
        private static Stopwatch _stopwatch = new Stopwatch();
        
        /// <summary>
        /// Starts the cycle timer.
        /// </summary>
        public static void Start()
        {
            _stopwatch.Restart();
        }
        
        /// <summary>
        /// Stops the cycle timer and returns the elapsed time in milliseconds.
        /// </summary>
        /// <returns>Elapsed time in milliseconds</returns>
        public static double Stop()
        {
            _stopwatch.Stop();
            return _stopwatch.Elapsed.TotalMilliseconds;
        }
        
        /// <summary>
        /// Gets the current elapsed time in milliseconds without stopping the timer.
        /// </summary>
        /// <returns>Current elapsed time in milliseconds</returns>
        public static double ElapsedMs()
        {
            return _stopwatch.Elapsed.TotalMilliseconds;
        }
        
        /// <summary>
        /// Resets the timer to zero.
        /// </summary>
        public static void Reset()
        {
            _stopwatch.Reset();
        }
    }
}