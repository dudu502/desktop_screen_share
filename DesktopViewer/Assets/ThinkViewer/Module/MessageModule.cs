using Think.Viewer.Manager;
using Think.Viewer.Event;
using UnityEngine;
using Think.Viewer.Common;
using Net;
using Protocol.Net;
using System.Collections.Generic;
using ThinkViewer.Scripts.Net.Data;
using UnityEngine.UIElements;
using System.Net;

namespace Think.Viewer.Module
{
    public enum MessageEvent
    {
        HostNetInfosUpdated,
    }
    public class MessageModule : IModule
    {
        public void Initialize()
        {
            EventDispatcher<S2C, PtMessagePackage>.AddListener(S2C.SearchHost, OnSearchHostResponse);
            EventDispatcher<S2C, PtMessagePackage>.AddListener(S2C.StartStreaming, OnStartStreamingResponse);
        }
        
        void OnSearchHostResponse(PtMessagePackage message)
        {
            using (ByteBuffer buffer = new ByteBuffer(message.Content))
            {
                string hostName = buffer.ReadString();
                string ip = buffer.ReadString();
                int hostPort = buffer.ReadInt32();
                int streamingPort = buffer.ReadInt32();
                var hostInfos = ModuleManager.GetModule<DataModule>().HostNetInfos;
                if(hostInfos.Find(item=>item.Ip == ip) == null)
                {
                    hostInfos.Add(new HostNetInfo(hostName,ip,hostPort,streamingPort));
                    UnityLooper.Execute(()=>EventDispatcher<MessageEvent, object>.DispatchEvent(MessageEvent.HostNetInfosUpdated, null));
                }
            }
        }
        void OnStartStreamingResponse(PtMessagePackage message)
        {
            using(ByteBuffer buffer = new ByteBuffer(message.Content))
            {
                string settingsJson = buffer.ReadString();
                ModuleManager.GetModule<DataModule>().HostSetting = LitJson.JsonMapper.ToObject<Setting>(settingsJson);
                Debug.LogWarning("Setting Info " + ModuleManager.GetModule<DataModule>().HostSetting.ToString());

                ModuleManager.GetModule<StreamingModule>().StartConnect((IPEndPoint)message.ExtraObj);
            }
        }
    }
}
