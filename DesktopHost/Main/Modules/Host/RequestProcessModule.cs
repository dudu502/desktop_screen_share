using Development.Net.Pt;
using Main.Ext;
using Protocol.Net;
using System.Net;
using Think.Viewer.Common;
using Think.Viewer.Core;
using Think.Viewer.Event;
using Think.Viewer.Modules;

namespace Main.Modules.Host
{
    public class Request
    {
        public Stream Stream { private set; get; }
        public int ProtocolId;
        public byte[] RawBytes;
        public Request(Stream stream, int pid, byte[] bytes)
        {
            Stream = stream;
            ProtocolId = pid; RawBytes = bytes;
        }
    }
    internal class RequestProcessModule : BaseModule
    {
        public RequestProcessModule(BaseApplication app) : base(app)
        {
            EventDispatcher<C2S, PtMessagePackage>.AddListener(C2S.SearchHost, OnSearchHost);
            EventDispatcher<C2S, PtMessagePackage>.AddListener(C2S.StartStreaming, OnStartStreaming);
            EventDispatcher<C2S, PtMessagePackage>.AddListener(C2S.StreamingOpLeftMouse, OnStreamOpLeftMouse);
            EventDispatcher<C2S, PtMessagePackage>.AddListener(C2S.StreamingOpRightMouse, OnStreamOpRightMouse);
            EventDispatcher<C2S, PtMessagePackage>.AddListener(C2S.ChangeQuality, OnChangeQuality);
        }
        
        void OnChangeQuality(PtMessagePackage package)
        {
            using (ByteBuffer buffer = new ByteBuffer(package.Content))
            {
                string uuid = buffer.ReadString();
                int quality = buffer.ReadInt32();
                var group = GetModule<CaptureHostModule>().GetStreamingProcessGroup(uuid);
                if(group != null )
                {
                    group.captureLooper.Enqueue(() =>
                    {
                        ScreenExt.ChangeQuality(quality);
                    });
                }
            }
        }
        void OnSearchHost(PtMessagePackage package)
        {
            string hostName = Global.HostName;
            string ip = Global.ServerIP;// + ":" + Global.setting.StreamingPort;
            int hostPort = Global.setting.HostPort;
            int streamingPort = Global.setting.StreamingPort;

            Relay(PtMessagePackage.BuildParams((ushort)S2C.SearchHost, hostName, ip, hostPort, streamingPort).SetToIp(package.FromIp).SetToPort(package.FromPort)
                .SetFromIp(IPAddress.Parse(ip).GetAddressBytes()).SetFromPort(hostPort));
            //package.Reply(GetNetManager(), PtMessagePackage.Write( PtMessagePackage.BuildParams((ushort)S2C.SearchHost, hostName,ip,hostPort,streamingPort)));
        }


        void OnStartStreaming(PtMessagePackage package)
        {
       
            using(ByteBuffer byteBuffer = new ByteBuffer(package.Content))
            {
                var uuid = byteBuffer.ReadString();
                Log($"{nameof(OnStartStreaming)} UUID:{uuid}");
                GetModule<CaptureHostModule>().TryStart(uuid);
            }
            //Start capture service
            string settingsJson = LitJson.JsonMapper.ToJson(Global.setting);
            Relay(PtMessagePackage.BuildParams((ushort)S2C.StartStreaming, settingsJson).SetToIp(package.FromIp).SetToPort(package.FromPort)
                .SetFromIp(IPAddress.Parse(Global.ServerIP).GetAddressBytes()).SetFromPort(Global.setting.HostPort));
            //package.Reply(GetNetManager(), PtMessagePackage.Write(PtMessagePackage.BuildParams((ushort)S2C.StartStreaming, settingsJson)));
        }

        void OnStreamOpLeftMouse(PtMessagePackage package)
        {
            PtStreamingOp op = PtStreamingOp.Read(package.Content);

            int x = (int)op.Position.X;
            int y = (int)(Global.setting.CaptureHeight - op.Position.Y);

            switch (op.OpType)
            {
                case Const.STREAMING_OP_CLICK:
                    MouseSim.MouseLeftClick(x, y);
                    break;
                case Const.STREAMING_OP_DOWN:
                    MouseSim.MouseLeftDown(x, y);
                    break;
                case Const.STREAMING_OP_UP:
                    MouseSim.MouseLeftUp(x, y);
                    break;
                case Const.STREAMING_OP_DOUBLE_CLICK:
                    MouseSim.MouseLeftDoubleClick(x, y);
                    break;
                case Const.STREAMING_OP_MOVE:
                    MouseSim.SetCursorPos(x, y);
                    break;
                default:
                    break;
            }
        }

        void OnStreamOpRightMouse(PtMessagePackage package)
        {
            PtStreamingOp op = PtStreamingOp.Read(package.Content);
      

            int x = (int)op.Position.X;
            int y = (int)(Global.setting.CaptureHeight - op.Position.Y);
            switch (op.OpType)
            {
                case Const.STREAMING_OP_CLICK:
                    MouseSim.MouseRightClick((int)op.Position.X, (int)y);
                    break;
                default:
                    break;
            }
              
        }
    }
}
