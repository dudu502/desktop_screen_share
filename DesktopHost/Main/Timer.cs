using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
    /// <summary>
    /// unit:ms
    /// </summary>
    public class Timer
    {
        private int startTime;
        public int Elapsed => DateTime.Now.Millisecond - startTime;

        public Timer()
        {
            Reset();
        }

        public void Reset()
        {
            startTime = DateTime.Now.Millisecond;
        }

        public static bool operator >(Timer timer, int duration)
            => timer.Elapsed > duration;

        public static bool operator <(Timer timer, int duration)
            => timer.Elapsed < duration;

        public static bool operator >=(Timer timer, int duration)
            => timer.Elapsed >= duration;

        public static bool operator <=(Timer timer, int duration)
            => timer.Elapsed <= duration;

        public static float operator /(Timer timer, int duration)
            => 1f*timer.Elapsed / duration;
    }
}
