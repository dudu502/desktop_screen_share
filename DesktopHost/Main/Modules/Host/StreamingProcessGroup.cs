using Common;
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

namespace Main.Modules.Host
{
    public class StreamRequest
    {
        public StreamWrapper StreamWrapper;
        public byte[] Raw;
        public string UUID;
        public StreamRequest(string uuid,StreamWrapper streamWrapper, byte[] raw)
        {
            UUID = uuid;
            StreamWrapper = streamWrapper;
            Raw = raw;
        }
    }
    public class StreamWrapper
    {
        public int Index = -1;
        public Stream NetStream { private set; get; }
        public StreamWrapper(Stream stream)
        {
            NetStream = stream;
        }
    }
    public class StreamingProcessGroup
    {
        public string UUID { private set; get; }
        public string StreamingHostIp { private set; get; }
        public int MultiThreadCount { private set; get; }
        public int StreamingPort { private set; get; }

        public List<StreamWrapper> NetStreams;
        public bool IsRunning;
        private TcpListener listener;
        private Thread tcpListenerThread;
        private Thread netStreamReceiveThread;
        private Thread netStreamSendThread;
        private readonly byte[] int32SizeBytes = new byte[4];
        private readonly byte[] int16SizeBytes = new byte[2];
        private readonly byte[] buffer = new byte[Const.BUFFER_SIZE];
        
        public StreamingProcessGroup(string uuid,string ip,int streamingPort,int multi_thread) 
        {
            UUID = uuid;
            StreamingHostIp = ip;
            StreamingPort = streamingPort;
            MultiThreadCount = multi_thread;
            NetStreams = new List<StreamWrapper>();
            listener = new TcpListener(IPAddress.Parse(StreamingHostIp), StreamingPort);
            listener.Start();
        }
        public void Start()
        {
            IsRunning = true;
            tcpListenerThread = new Thread(RunAcceptThread);
            tcpListenerThread.Start();
            netStreamReceiveThread = new Thread(RunNetStreamReceiveThread);       
            netStreamSendThread = new Thread(RunNetStreamSendThread1);
        }
        /// <summary>
        /// Size:4|PID:2|RawLen:4|RawBytes:N
        /// </summary>
        void RunNetStreamReceiveThread()
        {
            while(IsRunning)
            {
                foreach(var stream in NetStreams)
                {
                    if (stream.NetStream.CanRead)
                    {
                        byte[] receiveRaw = null;
                        stream.NetStream.Read(int32SizeBytes, 0, 4);
                        int totalLength = BitConverter.ToInt32(int32SizeBytes);
                        stream.NetStream.Read(int16SizeBytes, 0, 2);
                        ushort pid = BitConverter.ToUInt16(int16SizeBytes);
                        stream.NetStream.Read(int32SizeBytes, 0, 4);
                        int rawLength = BitConverter.ToInt32(int32SizeBytes);

                        int bytesRead = 0;
                        while(bytesRead < rawLength)
                        {
                            int thisRead = stream.NetStream.Read(buffer, 0, Math.Min(buffer.Length, rawLength - bytesRead));
                            if (receiveRaw == null)
                                receiveRaw = new byte[0];
                            Array.Resize(ref receiveRaw, thisRead+receiveRaw.Length);
                            Buffer.BlockCopy(buffer,0,receiveRaw,bytesRead,thisRead);
                            bytesRead += thisRead;
                        }
                        EventDispatcher<C2S, StreamRequest>.DispatchEvent((C2S)pid, new StreamRequest(UUID,stream, receiveRaw));
                    }
                }
            }
        }

        public void TryStartSendFrameThread()
        {
            bool allset = true;
            foreach(var item in NetStreams)
            {
                allset &= item.Index > -1;
            }
            if(allset)
                netStreamSendThread.Start();
        }

        void RunNetStreamSendThread1()
        {
            while(IsRunning)
            {
                byte[] screenStreamBytes = ScreenExt.BitBltCaptureScreenBytes();
                var stream = NetStreams[0].NetStream;
                stream.Write(BitConverter.GetBytes(screenStreamBytes.Length));
                stream.Write(screenStreamBytes);
                stream.Flush();
                Thread.Sleep(10);

            }

        }
        /// <summary>
        /// Send Screen preview
        /// RawLen:4|RawBytes:N
        /// </summary>
        void RunNetStreamSendThread()
        {
            while (IsRunning)
            {
                byte[] screenStreamBytes = ScreenExt.CaptureScreenBytes();
                if (screenStreamBytes != null)
                {
                    int screenRawLength = screenStreamBytes.Length;

                    byte[][] split = Utils.SplitByteArray(screenStreamBytes, MultiThreadCount);

                    for(int i=0; i<split.Length; i++)
                    {
                        var streamWrapper = GetNetStreamWrapper(i);
                        if (streamWrapper != null && streamWrapper.NetStream.CanWrite)
                        {
                            // 
                            var frameHash = screenStreamBytes.GetHashCode();
                            var frameHashBytes = BitConverter.GetBytes(frameHash);

                            var idxBytes = BitConverter.GetBytes(i);

                            var segBytes = split[i];

                            var idxSegLengthBytes = BitConverter.GetBytes(segBytes.Length);
                            var idxSegBytes = new byte[4 + 4 + 4 + segBytes.Length];
                            Buffer.BlockCopy(frameHashBytes, 0, idxSegBytes, 0, frameHashBytes.Length);
                            Buffer.BlockCopy(idxBytes, 0, idxSegBytes, frameHashBytes.Length, idxBytes.Length);
                            Buffer.BlockCopy(idxSegLengthBytes, 0, idxSegBytes, frameHashBytes.Length + idxBytes.Length, idxSegLengthBytes.Length);
                            Buffer.BlockCopy(segBytes, 0, idxSegBytes, frameHashBytes.Length + idxBytes.Length + idxSegLengthBytes.Length, segBytes.Length);
                            //



                            byte[] segRawLengthBytes = BitConverter.GetBytes(idxSegBytes.Length);

                            byte[] segFrameRaw = new byte[4 + idxSegBytes.Length];

                            Buffer.BlockCopy(segRawLengthBytes, 0, segFrameRaw, 0, segRawLengthBytes.Length);
                            Buffer.BlockCopy(idxSegBytes, 0, segFrameRaw, segRawLengthBytes.Length, idxSegBytes.Length);

                            //BaseApplication.Logger.Log($"Push Screen Seg {i} Length {segFrameRaw.Length}");



                            streamWrapper.NetStream.Write(segFrameRaw);
                            streamWrapper.NetStream.Flush();
                        }
                    }
                }
                Thread.Sleep(10);
            }
        }

        StreamWrapper GetNetStreamWrapper(int idx)
        {
            foreach(var item in NetStreams)
            {
                if(item.Index==idx)return item;
            }
            return null;
        }
             
        void RunAcceptThread()
        {
            while(IsRunning)
            {
                TcpClient client = listener.AcceptTcpClient();
                BaseApplication.Logger.Log("Accept "+client.ToString());
                var netStream = client.GetStream();
                NetStreams.Add(new StreamWrapper(netStream));
                if (NetStreams.Count == MultiThreadCount)
                {
                    netStreamReceiveThread.Start();
                }
            }
        }
    }
}
