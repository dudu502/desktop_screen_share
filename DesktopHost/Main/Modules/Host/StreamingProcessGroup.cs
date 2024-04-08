using Common;
using Main.Ext;
using Protocol.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        public bool WaitingAccept = true;
        private TcpListener listener;
        private Thread tcpListenerThread;
        private Thread netStreamReceiveThread;
        private Thread netStreamSendThread;
        private readonly byte[] int32SizeBytes = new byte[4];
        private readonly byte[] int16SizeBytes = new byte[2];
        private readonly byte[] buffer = new byte[Const.BUFFER_SIZE];

        public ConcurrentQueue<Action> captureLooper = new ConcurrentQueue<Action>();
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
                Thread.Sleep(10);
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
                while (captureLooper.TryDequeue(out var action))
                    action?.Invoke();
                AudioExt.TryStart();
                
                byte[] screenStreamBytes = ScreenExt.BitBltCaptureScreenBytes();
                var stream = NetStreams[0].NetStream;
                stream.WriteByte(0);
                stream.Write(BitConverter.GetBytes(screenStreamBytes.Length));
                stream.Write(screenStreamBytes);

                //byte[] audioBytes = AudioExt.CapturePeriodBytes();
                //if (audioBytes.Length > 0)
                //{
                //    stream.WriteByte(1);
                //    stream.Write(BitConverter.GetBytes(audioBytes.Length));
                //    BaseApplication.Logger.Log("SendAudio bytes " + audioBytes.Length);
                //    stream.Write(audioBytes);
                //}

                stream.Flush();
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
                while (captureLooper.TryDequeue(out var action))
                    action?.Invoke();
                // capture screen raw bytes;
                byte[] screenStreamBytes = ScreenExt.BitBltCaptureScreenBytes();

                byte[][] split = Utils.SplitByteArray(screenStreamBytes, MultiThreadCount);

                for (int i = 0; i < split.Length; i++)
                {
                    var streamWrapper = GetNetStreamWrapper(i);
                    if (streamWrapper != null && streamWrapper.NetStream.CanWrite)
                    {
                        // Write total Size bytes(i32)
                        byte[] totalSizeBytes = BitConverter.GetBytes(4 + 4 + 4 + split[i].Length);
                        // the Hashcode bytes of screen picture.(i32)
                        byte[] frameHashBytes = BitConverter.GetBytes(screenStreamBytes.GetHashCode());
                        // the Index bytes(i32)
                        byte[] idxBytes = BitConverter.GetBytes(i);
                        // one seg size bytes (i32)
                        byte[] segSizeByts = BitConverter.GetBytes(split[i].Length);
                        // one seg bytes;
                        byte[] segBytes = split[i];


                        byte[] frameBytes = new byte[4+BitConverter.ToInt32(totalSizeBytes)];
                        Buffer.BlockCopy(totalSizeBytes, 0, frameBytes, 0, totalSizeBytes.Length);
                        Buffer.BlockCopy(frameHashBytes,0,frameBytes,totalSizeBytes.Length, frameHashBytes.Length);
                        Buffer.BlockCopy(idxBytes, 0, frameBytes, totalSizeBytes.Length + frameHashBytes.Length, idxBytes.Length);
                        Buffer.BlockCopy(segSizeByts,0,frameBytes,totalSizeBytes.Length+frameHashBytes.Length+idxBytes.Length,segSizeByts.Length);
                        Buffer.BlockCopy(segBytes,0,frameBytes,totalSizeBytes.Length+frameHashBytes.Length+idxBytes.Length+segSizeByts.Length,segBytes.Length);

                        BaseApplication.Logger.Log($"Write Seg {i} Count {frameBytes.Length}");
                        streamWrapper.NetStream.WriteAsync(frameBytes);
                        streamWrapper.NetStream.FlushAsync();
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
            while(IsRunning && WaitingAccept)
            {
                TcpClient client = listener.AcceptTcpClient();
                BaseApplication.Logger.Log("Accept "+client.ToString());
                var netStream = client.GetStream();
                NetStreams.Add(new StreamWrapper(netStream));
                if (NetStreams.Count == MultiThreadCount)
                {
                    netStreamReceiveThread.Start();
                    WaitingAccept = false;
                }
            }
        }
    }
}
