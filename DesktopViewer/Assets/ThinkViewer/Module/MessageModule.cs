using Think.Viewer.Manager;
using Think.Viewer.Event;
using UnityEngine;
using Think.Viewer.Common;
using Net;
using Protocol.Net;
using System.Collections.Generic;
using ThinkViewer.Scripts.Net.Data;
using UnityEngine.UIElements;

namespace Think.Viewer.Module
{
    public enum MessageEvent
    {
        HostNetInfosUpdated,
    }
    public class MessageModule : IModule
    {
        public List<HostNetInfo> hostNetInfos = new List<HostNetInfo>();
        public void Initialize()
        {
            EventDispatcher<S2C, PtMessagePackage>.AddListener(S2C.SearchHost, OnSearchHostResponse);
        }
        
        void OnSearchHostResponse(PtMessagePackage message)
        {
            using (ByteBuffer buffer = new ByteBuffer(message.Content))
            {
                string hostName = buffer.ReadString();
                string ip = buffer.ReadString();
                int hostPort = buffer.ReadInt32();
                int streamingPort = buffer.ReadInt32();
     
                if(hostNetInfos.Find(item=>item.Ip == ip) == null)
                {
                    hostNetInfos.Add(new HostNetInfo(hostName,ip,hostPort,streamingPort));
                    UnityLooper.Execute(()=>EventDispatcher<MessageEvent, object>.DispatchEvent(MessageEvent.HostNetInfosUpdated, null));
                }
            }
        }

    }
}
