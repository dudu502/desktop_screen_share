using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Think.Viewer.FSM
{
    public class Timer
    {
        public float startTime;
        public float Elapsed => Time.time - startTime;

        public Timer()
        {
            startTime = Time.time;
        }

        public void Reset()
        {
            startTime = Time.time;
        }

        public static bool operator >(Timer timer, float duration)
            => timer.Elapsed > duration;

        public static bool operator <(Timer timer, float duration)
            => timer.Elapsed < duration;

        public static bool operator >=(Timer timer, float duration)
            => timer.Elapsed >= duration;

        public static bool operator <=(Timer timer, float duration)
            => timer.Elapsed <= duration;

        public static float operator /(Timer timer, float duration)
            => timer.Elapsed / duration;
    }
}
