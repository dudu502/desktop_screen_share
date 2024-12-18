using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using Think.Viewer.Module;
using UnityEngine;
using Think.Viewer.Net;
using Think.Viewer.Common;
using Think.Viewer.Recycling;
using Think.Viewer.Manager;
using Think.Viewer.Event;
using ThinkViewer.Scripts.Net.Data;
using Development.Net.Pt;

namespace Think.Viewer.UI
{
    public class WelcomeUI : UIView
    {
        private string hostName;
        private IPAddress interNetworkIp;
        private byte[] interNetworkIpBytes = null;
        public RecyclingListRenderer listRender;
        void Start()
        {
            hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            foreach (IPAddress ip in ipEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    interNetworkIp = ip;
                }
            }
            interNetworkIpBytes = interNetworkIp.GetAddressBytes();
            GameClientNetwork.Instance.localIp = interNetworkIpBytes;
            listRender.InitRendererList(OnSelectRender);
            listRender.SetDataProvider(ModuleManager.GetModule<DataModule>().HostNetInfos);
            EventDispatcher<MessageEvent, object>.AddListener(MessageEvent.HostNetInfosUpdated, OnUpdateHostInfos);
        }
        void OnUpdateHostInfos(object value)
        {
            listRender.RefreshDataProvider();
        }
        void OnSelectRender(RecyclingItem.RecyclingEvent evt)
        {
            switch (evt.Type)
            {
                case HostItem.Start_Streaming:
                    HostNetInfo hostNetInfo = evt.Target as HostNetInfo;
                    GameClientNetwork.Instance.SendUnconnectedRequestRaw(PtMessagePackage.BuildParams((ushort)C2S.StartStreaming,Application.UUID).SetToPort(hostNetInfo.EndPoint.Port).SetToIp(hostNetInfo.EndPoint.Address.GetAddressBytes()).SetFromIp(interNetworkIpBytes).SetFromPort(50000));
                    break;
                default:
                    break;
            }
        }
        public void OnClickSearch()
        {
            Debug.LogWarning("OnClickSearch");
            for(int i = 1; i < 256; ++i)
            {
                IPAddress ip = IPAddress.Parse($"{interNetworkIpBytes[0]}.{interNetworkIpBytes[1]}.{interNetworkIpBytes[2]}.{i}");
                GameClientNetwork.Instance.SendUnconnectedRequestRaw(PtMessagePackage.Build((ushort)C2S.SearchHost).SetToPort(7999).SetToIp(ip.GetAddressBytes()).SetFromIp(interNetworkIpBytes).SetFromPort(50000));
            }
        }
        public override void OnInit()
        {
            base.OnInit();
        }
        void Update()
        {

        }
    }
}