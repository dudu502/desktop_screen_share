using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Think.Viewer.Common;

namespace Think.Viewer
{
    public class ConsoleLogger:ILogger
    {
        private object SyncObj = new object();

        public bool EnableConsoleOutput = true;

        private readonly string _Tag;

        private readonly string _TimeFormatPatten = "yyyy/MM/dd HH:mm:ss.fff";

        private string LogServerUrl;

        private int BufferSize;

        private Queue<string> _QueueLogs = new Queue<string>();
        public ConsoleLogger(string tag, string logServerUrl, int bufferSize = 16)
        {
            _Tag = tag;
            LogServerUrl = logServerUrl;
            BufferSize = bufferSize;
        }

        public void Log(string msg)
        {
            SendLogToLogServer("[" + DateTime.Now.ToString(_TimeFormatPatten) + "]I[" + _Tag + "]\t" + msg);
        }

        public void LogError(string msg)
        {
            SendLogToLogServer("[" + DateTime.Now.ToString(_TimeFormatPatten) + "]E[" + _Tag + "]\t" + msg);
        }

        public void LogWarning(string msg)
        {
            SendLogToLogServer("[" + DateTime.Now.ToString(_TimeFormatPatten) + "]W[" + _Tag + "]\t" + msg);
        }

        private void SendLogToLogServer(string logMessage)
        {
            //if (!string.IsNullOrEmpty(LogServerUrl))
            //{
            //    lock (SyncObj)
            //    {
            //        if (_QueueLogs.Count > BufferSize)
            //        {
            //            AsyncHttpTask.HttpGetRequest(LogServerUrl + string.Join("\n", _QueueLogs), (Action<string>)null, (Action<Exception>)null);
            //            _QueueLogs.Clear();
            //        }
            //        else
            //        {
            //            _QueueLogs.Enqueue(logMessage);
            //        }
            //    }
            //}

            if (EnableConsoleOutput)
            {
                Console.WriteLine(logMessage);
            }
        }

    }
}
