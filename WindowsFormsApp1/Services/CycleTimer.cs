using System.Diagnostics;

namespace WindowsFormsApp1.Services
{
    public static class CycleTimer
    {
        private static readonly Stopwatch stopwatch = new Stopwatch();

        public static void Start() => stopwatch.Restart();

        public static double Stop()
        {
            if (stopwatch.IsRunning)
                stopwatch.Stop();
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        public static double GetElapsed()
        {
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        public static void Reset() => stopwatch.Reset();
    }
}
