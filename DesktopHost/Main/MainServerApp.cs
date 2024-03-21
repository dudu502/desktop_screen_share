using LiteNetLib;
using Main.Ext;
using Main.Modules.Host;
using Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Think.Viewer.Common;
using Think.Viewer.Core;
using Think.Viewer.Event;
using Think.Viewer.Modules;

namespace Main
{
    public class MainServerApp : BaseApplication
    {
        public const int MAX_CONNECT_COUNT = 128;
        public MainServerApp(string key) : base(key)
        {
            m_Network.UnconnectedMessagesEnabled = true;
        }

        protected override void SetUp()
        {
            base.SetUp();
            SetUpConfigs();
            AddModule(new CaptureHostModule(this));
            AddModule(new RequestProcessModule(this));
            AddModule(new HeartbeatModule(this));
        }
        void SetUpConfigs()
        {
            Global.setting = LitJson.JsonMapper.ToObject<Setting>(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "setting.json")));
            Logger.Log(Global.setting.ToString());

            var hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            foreach (IPAddress ip in ipEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Global.ServerIP = ip.ToString();
                }
            }
            Global.HostName = hostName;
            ScreenExt.Init(Global.setting.GetCaptureRectangle(), (long)Convert.ToInt32(Global.setting.StreamingQuality));
        }
        protected override void OnConnectionRequestEvent(ConnectionRequest request)
        {
            base.OnConnectionRequestEvent(request);
        }
        protected override void OnNetworkReceiveUnconnectedEvent(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            base.OnNetworkReceiveUnconnectedEvent(remoteEndPoint, reader, messageType);
            byte[] bytes = new byte[reader.AvailableBytes];
            reader.GetBytes(bytes, reader.AvailableBytes);
            Logger.Log($"Receive unconnected event {remoteEndPoint}");
            PtMessagePackage package = PtMessagePackage.Read(bytes);
            EventDispatcher<C2S, UnconnectedNetMessageEvt>.DispatchEvent((C2S)package.MessageId, new UnconnectedNetMessageEvt(remoteEndPoint, package.Content));
            reader.Recycle();
        }
        protected override void OnPeerConnectedEvent(NetPeer peer)
        {
            base.OnPeerConnectedEvent(peer);
            Logger.Log(string.Format("PeerId:{0} Connected.[{1}/{2}]", peer.Id, m_Network.ConnectedPeerList.Count, MAX_CONNECT_COUNT));
        }
        protected override void OnPeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            base.OnPeerDisconnectedEvent(peer, disconnectInfo);
            Logger.LogWarning(string.Format("PeerId:{0} DisConnected.[{1}/{2}] DisconnectReason:{3} SocketError:{4}", peer.Id, m_Network.ConnectedPeerList.Count, MAX_CONNECT_COUNT, disconnectInfo.Reason.ToString(), disconnectInfo.SocketErrorCode.ToString()));
        }
    }
}
