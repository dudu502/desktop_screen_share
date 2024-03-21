using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Think.Viewer
{
    public class UnityLooper:MonoBehaviour
    {
        public static ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();
        private void Update()
        {
            while(actions.TryDequeue(out Action action))
            {
                action?.Invoke();
            }
        }
        public static void Execute(Action action)
        {
            actions.Enqueue(action);
        }
    }
}
