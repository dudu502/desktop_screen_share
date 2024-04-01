
using Think.Viewer.Common;
using Think.Viewer.Core;
using Think.Viewer.Event;
using Think.Viewer.Modules;

namespace Main.Modules.Host
{
    public class CaptureHostModule : BaseModule
    {
        public bool IsRunning = false;
        private StreamingProcessGroup streamingProcessGroup;

        public CaptureHostModule(BaseApplication app) : base(app)
        {
            EventDispatcher<C2S, StreamRequest>.AddListener(C2S.SetStreamingIndex, OnSetStreamIndexRequest);
        }
        public StreamingProcessGroup GetStreamingProcessGroup(string uuid)
        {
            if(streamingProcessGroup != null && streamingProcessGroup.UUID == uuid) 
                return streamingProcessGroup;
            return null;
        }
        void OnSetStreamIndexRequest(StreamRequest request)
        {
            int idx= BitConverter.ToInt32(request.Raw, 0);
            request.StreamWrapper.Index = idx;
            if (streamingProcessGroup != null && streamingProcessGroup.UUID == request.UUID)
            {
                streamingProcessGroup.TryStartSendFrameThread();
            }
        }

        public void TryStart(string uuid)
        {
            if (streamingProcessGroup == null)
                streamingProcessGroup = new StreamingProcessGroup(uuid, Global.ServerIP, Global.setting.StreamingPort, Global.setting.MultiThreadCount);
            if (!streamingProcessGroup.IsRunning)
                streamingProcessGroup.Start();
        }
      
   



        //void RequestThreadStart()
        //{
        //    while (running && !accept)
        //    {
        //        Console.WriteLine("Client waiting");
        //        TcpClient tcpClient = listener.AcceptTcpClient();
        //        Console.WriteLine("Client Accpet");
        //        stream = tcpClient.GetStream();
        //        accept = true;

        //        // save client to dict;

        //        //while (true)
        //        //{
        //        //    var screenStreamBytes = ScreenExt.CaptureScreenBytes();
        //        //    var totalBytes = screenStreamBytes;
        //        //    stream.Write(BitConverter.GetBytes(totalBytes.Length));


        //        //    //int offset = 0;
        //        //    //while (offset < totalBytes.Length)
        //        //    //{
        //        //    //    int bytesToSend = Math.Min(Const.BUFFER_SIZE, totalBytes.Length - offset);
        //        //    //    stream.Write(totalBytes, offset, bytesToSend);
        //        //    //    offset += bytesToSend;
        //        //    //}
        //        //    stream.Write(totalBytes);
        //        //    stream.Flush();
        //        //}
        //    }
        //}
    }
}
