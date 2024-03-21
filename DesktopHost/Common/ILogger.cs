using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Think.Viewer.Common
{
    public interface ILogger
    {
        void Log(string msg);
        void LogWarning(string msg);
        void LogError(string msg);
    }
}
