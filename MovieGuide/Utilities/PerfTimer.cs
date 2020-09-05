using System;

namespace MovieGuide
{
    /// <summary>
    /// Lightweight/Fast Performance Timer. Similar to System.Diagnostics.Stopwatch().
    /// Limitations: Strange results may occur if your computer is continously running for over 49 days (e.g. integer rollover)
    /// </summary>
    public class PerfTimer
    {
        private UInt32 t1 = TickCount();
        private bool paused = false;
        private UInt32 paused_delta = 0;

        public PerfTimer() { }

        private static UInt32 TickCount()
        {
            unchecked
            {
                return (UInt32)Environment.TickCount;
            }
        }

        /// <summary>
        /// Return the current elapsed time in milliseconds. Does not stop the timer.
        /// </summary>
        public UInt32 Count
        {
            get
            {
                if (paused) return paused_delta;
                return paused_delta + (TickCount() - t1);
            }
        }

        /// <summary>
        /// Reset the timer count to zero, but continue timing.
        /// </summary>
        public void Restart()
        {
            paused = false;
            paused_delta = 0;
            t1 = TickCount();
        }

        /// <summary>
        /// Reset the timer to zero. Timer is stopped.
        /// </summary>
        public void Stop()
        {
            paused = true;
            paused_delta = 0;
        }

        /// <summary>
        /// Starts the timer count from zero.
        /// </summary>
        public void Start()
        {
            if (paused)
            {
                paused = false;
                paused_delta = 0;
                t1 = TickCount();
            }
        }

        /// <summary>
        /// Pause the timer count but does not reset it.
        /// </summary>
        public void Pause()
        {
            if (!paused)
            {
                paused = true;
                paused_delta += (TickCount() - t1);
            }
        }

        /// <summary>
        /// Resumes the timer count
        /// </summary>
        public void Continue()
        {
            if (paused)
            {
                paused = false;
                t1 = TickCount();
            }
        }

        /// <summary>
        /// Print formatted current elapsed time message to Log. 
        /// Does not stop or pause the timer. Message is prefixed with "Performance: "
        /// </summary>
        /// <param name="format">message string format. see String.Format()</param>
        /// <param name="args">variable format args</param>
        public void LogPrint(string format, params object[] args)
        {
            Log.Write(Severity.Verbose, "Performance: " + format + string.Format(": {0} ms", this.Count), args);
        }

        /// <summary>
        /// Print current elapsed time message to debug output window. 
        /// Does not stop or pause the timer.
        /// </summary>
        /// <param name="msg">string message to print with elapsed time appended"</param>
        public void Print(string msg)
        {
            Diagnostics.WriteLine("{0}: {1} ms", msg, this.Count);
        }

        /// <summary>
        /// Print formatted current elapsed time message to debug output window. 
        /// Does not stop or pause the timer.
        /// </summary>
        /// <param name="format">message string format. see String.Format()</param>
        /// <param name="args">variable format args</param>
        public void Print(string format, params object[] args)
        {
            Diagnostics.WriteLine(format + string.Format(": {0} ms", this.Count), args);
        }
    }
}
