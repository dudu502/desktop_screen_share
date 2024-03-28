using Development.Net.Pt;
using Main.Ext;
using Net;
using Protocol.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            EventDispatcher<C2S, UnconnectedNetMessageEvt>.AddListener(C2S.SearchHost, OnSearchHost);
            EventDispatcher<C2S, UnconnectedNetMessageEvt>.AddListener(C2S.StartStreaming, OnStartStreaming);
            EventDispatcher<C2S, UnconnectedNetMessageEvt>.AddListener(C2S.StreamingOpLeftMouse, OnStreamOpLeftMouse);
            EventDispatcher<C2S, UnconnectedNetMessageEvt>.AddListener(C2S.StreamingOpRightMouse, OnStreamOpRightMouse);
            EventDispatcher<C2S, UnconnectedNetMessageEvt>.AddListener(C2S.ChangeQuality, OnChangeQuality);
        }
        
        void OnChangeQuality(UnconnectedNetMessageEvt package)
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
        void OnSearchHost(UnconnectedNetMessageEvt package)
        {
            Log($"{nameof(OnSearchHost)} {package.RemoteEndPoint.ToString()}");
            string hostName = Global.HostName;
            string ip = Global.ServerIP;// + ":" + Global.setting.StreamingPort;
            int hostPort = Global.setting.HostPort;
            int streamingPort = Global.setting.StreamingPort;
          
            package.Reply(GetNetManager(), PtMessagePackage.Write( PtMessagePackage.BuildParams((ushort)S2C.SearchHost, hostName,ip,hostPort,streamingPort)));
        }


        void OnStartStreaming(UnconnectedNetMessageEvt package)
        {
            Log($"{nameof(OnStartStreaming)} {package.RemoteEndPoint.ToString()}");
            using(ByteBuffer byteBuffer = new ByteBuffer(package.Content))
            {
                var uuid = byteBuffer.ReadString();
                Log($"{nameof(OnStartStreaming)} UUID:{uuid}");
                GetModule<CaptureHostModule>().TryStart(uuid);
            }
            //Start capture service
            string settingsJson = LitJson.JsonMapper.ToJson(Global.setting);
            package.Reply(GetNetManager(), PtMessagePackage.Write(PtMessagePackage.BuildParams((ushort)S2C.StartStreaming, settingsJson)));
        }

        void OnStreamOpLeftMouse(UnconnectedNetMessageEvt package)
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

        void OnStreamOpRightMouse(UnconnectedNetMessageEvt package)
        {
            PtStreamingOp op = PtStreamingOp.Read(package.Content);
            Log($"{nameof(OnStreamOpRightMouse)} {package.RemoteEndPoint.ToString()} {op.OpType}");

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
