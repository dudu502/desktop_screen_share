using Development.Net.Pt;
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
            EventDispatcher<C2S, UnconnectedNetMessageEvt>.AddListener(C2S.StreamingOpLeftClick, OnStreamOpLeftClick);
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

        void OnStreamOpLeftClick(UnconnectedNetMessageEvt package)
        {
            Log($"{nameof(OnStreamOpLeftClick)} {package.RemoteEndPoint.ToString()}");
            PtStreamingOp op = PtStreamingOp.Read(package.Content);
            float y = Global.setting.CaptureHeight - op.Position.Y;
   
            MouseSim.MouseClick((int)op.Position.X, (int)y);
        }

    }
}
