using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkViewer.Scripts.Net.Data
{
    public class HostNetInfo
    {
        public string Ip;
        public string HostName;

        public HostNetInfo(string ip,string hostName)
        {
            Ip = ip;
            HostName = hostName;
        }
    }
}
