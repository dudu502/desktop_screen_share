

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Think.Viewer.Common;
using UnityEngine;

namespace Think.Viewer.Scripts.Net
{
    public class Response
    {
        public int ProtocolId;
        public int ClientId;
        public byte[] RawBytes;

        public Response(int protocolId, int clientId, byte[] rawBytes)
        {
            ProtocolId = protocolId;
            ClientId = clientId;
            RawBytes = rawBytes;
        }
    }
    public class NetworkController
    {
        public ConcurrentQueue<Response> responseQueue = new ConcurrentQueue<Response>();
        TcpClient tcpClient;
        Stream stream;
        Thread receiveThread;
        public NetworkController()
        {
            tcpClient = new TcpClient();
            receiveThread = new Thread(OnReceiveThreadStart);
        }

        public async void Connect(string ip, int port)
        {
            Debug.LogError("0Connect " + tcpClient.Connected);
            await tcpClient.ConnectAsync(ip, port);
            Debug.LogError("1Connect "+tcpClient.Connected);
            receiveThread.Start();
            stream = tcpClient.GetStream();
    
            //SendRaw(ProtocolId.ConnectConfirm, Encoding.UTF8.GetBytes("hello server!"));
        }
        void OnReceiveThreadStart()
        {
            while (stream != null && stream.CanRead)
            {
                byte[] buffer4 = new byte[4];
                stream.Read(buffer4, 0, 4);
                int pid = stream.Read(buffer4, 0, 4);
                int clientId = stream.Read(buffer4, 0, 4);
                int rawLen = stream.Read(buffer4, 0, 4);

                byte[] buffer = new byte[Const.BUFFER_SIZE];
                int bytesRead = 0;
                byte[] frameRaw = null;
                while (bytesRead < rawLen)
                {
                    int thisread = stream.Read(buffer,0,Math.Min(buffer.Length,rawLen-bytesRead));
                    if (frameRaw == null)
                        frameRaw = new byte[0];
                    Array.Resize(ref frameRaw, frameRaw.Length+thisread);
                    Buffer.BlockCopy(buffer,0,frameRaw, bytesRead, thisread);
                    bytesRead += thisread;
                }
                responseQueue.Enqueue(new Response(pid,clientId,frameRaw));
            }
        }
        /// <summary>
        /// [4:TotalSize|4:PID|4:RawDataSize][RawData]
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="raw"></param>
        public void SendRaw(int pid, byte[] raw)
        {
            if(tcpClient.Connected && stream!=null&&stream.CanWrite)
            {
                int rawSize = 0;
                if(raw!=null)
                    rawSize = raw.Length;
                stream.Write(BitConverter.GetBytes(4+4+ rawSize));
                stream.Write(BitConverter.GetBytes(pid));
                stream.Write(BitConverter.GetBytes(rawSize));
                if (rawSize > 0)
                {
                    stream.Write(raw, 0, rawSize);
                }
                stream.Flush();
            }
        }
    }
}
