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
        public string hostName;
        public IPAddress interNetworkIp;
        TcpListener listener;
        public Thread requestThread;
        public Thread captureScreenThread;
        public Thread receiveThread;
        bool running;
        public bool streaming;
        Stream stream;
        public CaptureHostModule(BaseApplication app) : base(app)
        {
            hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            foreach (IPAddress ip in ipEntry.AddressList)
            {
                // 筛选出IPv4地址
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    interNetworkIp = ip;
                }
            }
        }

        public void Start()
        {
            int port = 8000;
            if (Global.setting != null)
                port = Global.setting.HostPort;
            listener = new TcpListener(interNetworkIp, port);
            listener.Start();
            running = true;
            requestThread = new Thread(RequestThreadStart);
            requestThread.Start();
            captureScreenThread = new Thread(CaptureScreenThreadStart);
            captureScreenThread.Start();
            receiveThread = new Thread(ReceiveThreadStart);

        }
        void ReceiveThreadStart()
        {
            while (running)
            {
                if (stream != null && stream.CanRead)
                {
                    byte[] bytes4 = new byte[4];
                    try
                    {
                        stream.Read(bytes4, 0, 4);
                        stream.Read(bytes4, 0, 4);
                        int pid = BitConverter.ToInt32(bytes4, 0);
                        stream.Read(bytes4, 0, 4);
                        int rawLen = BitConverter.ToInt32(bytes4, 0);
                        byte[] raw = null;
                        if (rawLen > 0)
                        {
                            raw = new byte[rawLen];
                            stream.Read(raw, 0, rawLen);
                        }
                        EventDispatcher<int, Request>.DispatchEvent(pid, new Request(stream, pid, raw));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
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
                receiveThread.Start();
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
