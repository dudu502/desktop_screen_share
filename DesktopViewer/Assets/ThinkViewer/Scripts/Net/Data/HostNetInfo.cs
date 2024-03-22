using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ThinkViewer.Scripts.Net.Data
{
    public class HostNetInfo
    {
        public string Ip;
        public string HostName;
        public int HostPort;
        public int StreamingPort;

        public IPEndPoint EndPoint { private set; get; }
        public HostNetInfo(string hostName, string ip,  int hostPort, int streamingPort)
        {
            Ip = ip;
            HostName = hostName;
            HostPort = hostPort;
            StreamingPort = streamingPort;

            EndPoint = new IPEndPoint(IPAddress.Parse(ip), hostPort);
        }
    }
}
