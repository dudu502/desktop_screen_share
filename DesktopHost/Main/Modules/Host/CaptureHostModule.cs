using Main.Ext;
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

namespace Main.Modules.Host
{
    public class CaptureHostModule : BaseModule
    {
        TcpListener listener;
        Thread requestThread;
        Thread captureScreenThread;

        bool running;
        public bool streaming;
        Stream stream;
 
        public CaptureHostModule(BaseApplication app) : base(app)
        {
            requestThread = new Thread(RequestThreadStart); 
            captureScreenThread = new Thread(CaptureScreenThreadStart);
        }

        public void Start()
        {
            int port = 8000;
            if (Global.setting != null)
                port = Global.setting.StreamingPort;
            listener = new TcpListener(IPAddress.Parse(Global.ServerIP), port);
            listener.Start();
            running = true;
            requestThread.Start();
            captureScreenThread.Start();
        }
      
        void CaptureScreenThreadStart()
        {
            while (running && streaming)
            {
                var screenStreamBytes = ScreenExt.CaptureScreenBytes();
                SendRaw(stream, (int)S2C.Streaming, 0, screenStreamBytes);
            }
        }
        /// <summary>
        /// [4:TotalSize|4:PID|4:ClientID|4:RawFrameSize|N:RawFrameBytes]
        /// </summary>
        /// <param name="data"></param>
        public void SendRaw(Stream stream, int pid, int clientId, byte[] raw)
        {
            if (stream != null && stream.CanWrite)
            {
                stream.Write(BitConverter.GetBytes(4 + 4 + 4 + raw.Length));
                stream.Write(BitConverter.GetBytes(pid));
                stream.Write(BitConverter.GetBytes(clientId));
                stream.Write(BitConverter.GetBytes(raw.Length));
                stream.Write(raw, 0, raw.Length);
                stream.Flush();
            }
        }

        bool accept = false;



        void RequestThreadStart()
        {
            while (running && !accept)
            {
                Console.WriteLine("Client waiting");
                TcpClient tcpClient = listener.AcceptTcpClient();
                Console.WriteLine("Client Accpet");
                stream = tcpClient.GetStream();
                accept = true;

                // save client to dict;

                //while (true)
                //{
                //    var screenStreamBytes = ScreenExt.CaptureScreenBytes();
                //    var totalBytes = screenStreamBytes;
                //    stream.Write(BitConverter.GetBytes(totalBytes.Length));


                //    //int offset = 0;
                //    //while (offset < totalBytes.Length)
                //    //{
                //    //    int bytesToSend = Math.Min(Const.BUFFER_SIZE, totalBytes.Length - offset);
                //    //    stream.Write(totalBytes, offset, bytesToSend);
                //    //    offset += bytesToSend;
                //    //}
                //    stream.Write(totalBytes);
                //    stream.Flush();
                //}
            }
        }
    }
}
