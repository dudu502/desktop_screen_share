using Main.Ext;
using Net;
using System;
using System.Collections.Concurrent;
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

namespace Main.Modules.Host
{
    public class CaptureHostModule : BaseModule
    {
        public bool IsRunning = false;
        private Dictionary<string, StreamingProcessGroup> streamingProcessDict;
 
        public CaptureHostModule(BaseApplication app) : base(app)
        {
            streamingProcessDict = new Dictionary<string, StreamingProcessGroup>();
            EventDispatcher<C2S, StreamRequest>.AddListener(C2S.SetStreamingIndex, OnSetStreamIndexRequest);
        }
        public StreamingProcessGroup GetStreamingProcessGroup(string uuid)
        {
            if(streamingProcessDict.ContainsKey(uuid))
            {
                return streamingProcessDict[uuid];
            }
            return null;
        }
        void OnSetStreamIndexRequest(StreamRequest request)
        {
            int idx= BitConverter.ToInt32(request.Raw, 0);
            Log("OnSetStreamIndexRequest "+idx);
            request.StreamWrapper.Index = idx;
            if (streamingProcessDict.TryGetValue(request.UUID,out var group))
            {
                group.TryStartSendFrameThread();
            }
        }

        public void TryStart(string uuid)
        {
            if(!streamingProcessDict.ContainsKey(uuid))
            {
                streamingProcessDict[uuid] = new StreamingProcessGroup(uuid,Global.ServerIP, Global.setting.StreamingPort, Global.setting.MultiThreadCount);
            }
            if (!streamingProcessDict[uuid].IsRunning)
            {
                streamingProcessDict[uuid].Start();
            }
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
