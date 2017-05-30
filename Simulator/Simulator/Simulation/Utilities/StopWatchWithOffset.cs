using System;
using System.Diagnostics;

namespace Simulator.Simulation.Utilities
{
    /// <summary>
    /// Helperclass for using a stopwatch with a defined offset
    /// </summary>
    public class StopWatchWithOffset
    {
        private Stopwatch _stopwatch = null;
        TimeSpan _offsetTimeSpan;

        /// <summary>
        /// Constructor which needs the wanted offset
        /// </summary>
        public StopWatchWithOffset(TimeSpan offsetElapsedTimeSpan)
        {
            _offsetTimeSpan = offsetElapsedTimeSpan;
            _stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Method for starting the stopwatch
        /// </summary>
        public void Start()
        {
            _stopwatch.Start();
        }

        /// <summary>
        /// Method for stopping the stopwatch
        /// </summary>
        public void Stop()
        {
            _stopwatch.Stop();
        }

        /// <summary>
        /// Field which contains the elapsed timespan.
        /// </summary>
        /// <param name="value">setter</param>
        /// <returns>Timespan -> getter</returns>
        public TimeSpan ElapsedTimeSpan
        {
            get
            {
                return _stopwatch.Elapsed + _offsetTimeSpan;
            }
            set
            {
                _offsetTimeSpan = value;
            }
        }
    }
}
