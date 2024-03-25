using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Think.Viewer.Common;
using Think.Viewer.Manager;
using ThinkViewer.Scripts.Net.Data;

namespace Think.Viewer.Module
{
    public class DataModule : IModule
    {
        public Setting HostSetting;
        public List<HostNetInfo> HostNetInfos = new List<HostNetInfo>();
        public ConcurrentQueue<byte[]> StreamingRawFrameQueue = new System.Collections.Concurrent.ConcurrentQueue<byte[]>();
        public void Initialize()
        {
            
        }
    }
}
