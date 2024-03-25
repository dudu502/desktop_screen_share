﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Think.Viewer.Common;
using Think.Viewer.Manager;
using Unity.VisualScripting;
using UnityEngine;

namespace Think.Viewer.Module
{
    public class StreamWrapper
    {
        public int Index;
        public TcpClient Client;
        public static readonly byte[] Int32Bytes = new byte[4]; 
        public static readonly byte[] Int16Bytes = new byte[2];
        public byte[] buffer = new byte[Const.BUFFER_SIZE];
        public Thread receiveThread;
        public StreamWrapper(int idx,TcpClient client)
        {
            Index = idx;
            Client = client;
            receiveThread = new Thread(RunReceiveThread);
        }
        public void StartThread()
        {
            receiveThread.Start();
        }

        void RunReceiveThread()
        {
            while (true)
            {
                Receive();
            }
        }
        public void Receive()
        {
            var stream = Client.GetStream();
            if (Client.Connected && stream.CanRead)
            {
                byte[] frameRaw = null;
                //stream.Read(Int32Bytes, 0, 4);
              

                stream.Read(Int32Bytes, 0, 4);
                int segRawLen = BitConverter.ToInt32(Int32Bytes);
                //Debug.LogError($"Receive Idx {Index} SegTotalLen:{BitConverter.ToInt32(Int32Bytes)}");

                int bytesRead = 0;
                while( bytesRead < segRawLen)
                {
                    int thisRead = stream.Read(buffer, 0, Math.Min(buffer.Length, segRawLen - bytesRead));
                    if (frameRaw == null)
                        frameRaw = new byte[0];
                    //Debug.LogWarning("Frame Start Read seg [[ Resize to " + (frameRaw.Length + thisRead));
                    Array.Resize(ref frameRaw, frameRaw.Length + thisRead);
                    //Debug.LogWarning("Frame Start Read seg [[ Resized to " + (frameRaw.Length) + $" CopyOp bufferSize:{buffer.Length} frameRawSize:{frameRaw.Length} dstOffset:{0 + bytesRead} count:{thisRead}");
                    Buffer.BlockCopy(buffer, 0, frameRaw, bytesRead, thisRead);
                    //Debug.LogWarning("Frame Start Read seg [[ BlockCopy " + frameRaw.Length);
                    bytesRead += thisRead;
                    //Debug.LogWarning("Frame Start Read seg ] " + bytesRead);
                }
                //seg loaded
                //Debug.LogWarning("Receive Seg Count"+frameRaw.Length);
                if (frameRaw != null)
                {
                    Debug.LogWarning($"Receive Idx {Index} Seg Count {frameRaw.Length}");

                    //
                    var rawImgBytes = new byte[frameRaw.Length-4-4-4];
                    Array.Copy(frameRaw, 4 + 4 + 4, rawImgBytes, 0, rawImgBytes.Length);
                    ModuleManager.GetModule<DataModule>().StreamingRawFrameQueue.Enqueue(rawImgBytes);
                }
            }
        }
    }
    public class StreamingModule : IModule
    {
        List<StreamWrapper> tcpStreamingClients;

        public void Initialize()
        {
            tcpStreamingClients = new List<StreamWrapper>();
        }

        /// <summary>
        /// Size:4|PID:2|RawLen:4|RawBytes:N
        /// </summary>
        /// <param name="client"></param>
        /// <param name="id"></param>
        /// <param name="raw"></param>
        private async void Send(TcpClient client,C2S id, byte[] raw)
        {
            int totalLength = 4 + 2 + 4;
            if(raw != null) 
                totalLength+= raw.Length;
            byte[] result = new byte[totalLength];
            byte[] sizeBytes = BitConverter.GetBytes(totalLength);
            byte[] pidBytes = BitConverter.GetBytes((ushort)id);
            byte[] rawLengthBytes = BitConverter.GetBytes(raw.Length);
            Buffer.BlockCopy(sizeBytes, 0, result, 0, sizeBytes.Length);
            Buffer.BlockCopy(pidBytes,0,result,sizeBytes.Length,pidBytes.Length);
            Buffer.BlockCopy(rawLengthBytes,0,result,sizeBytes.Length+pidBytes.Length,rawLengthBytes.Length);   
            Buffer.BlockCopy(raw,0,result,sizeBytes.Length+pidBytes.Length+rawLengthBytes.Length,raw.Length);
            if (client.GetStream().CanWrite)
            {
                await client.GetStream().WriteAsync(result);
            }
        }

        public async void StartConnect(IPEndPoint serverPoint)
        {
            var settings = ModuleManager.GetModule<DataModule>().HostSetting;
            if(settings != null)
            {
                for(int i=0;i<settings.MultiThreadCount;i++)
                {
                    StreamWrapper streamWrapper = new StreamWrapper(i, new TcpClient());                 
                    tcpStreamingClients.Add(streamWrapper);
                    await streamWrapper.Client.ConnectAsync(serverPoint.Address, settings.StreamingPort);
                    Debug.LogWarning("StartConnect "+"address"+serverPoint.Address.ToString()+"port:"+ settings.StreamingPort + " total "+ settings.MultiThreadCount);
                 
                }
            }

            var connectResult = true;
            foreach(StreamWrapper tcpClient in tcpStreamingClients)
                connectResult&=tcpClient.Client.Connected;
            Debug.LogWarning("All Streaming Request client connect result : "+connectResult);

            if (!connectResult)
            {
                Debug.LogError("Server Connect Error");
                return;
            }

            await Task.Delay(1000);
            // Set Flag to each client;

            for (int i=0;i<tcpStreamingClients.Count ;i++)
            {
                tcpStreamingClients[i].StartThread();
                Send(tcpStreamingClients[i].Client,C2S.SetStreamingIndex,BitConverter.GetBytes(i));
            }
            UnityLooper.Execute(() =>
            {
                ModuleManager.GetModule<UIModule>().Push(UITypes.StreamingUI, Layer.Default);
            });
     
        }


        void RunReceiveStreamingThread()
        {
            while (true)
            {
                foreach(var tcpClient in tcpStreamingClients)
                {
                    tcpClient.Receive();
                }
            }
        }
    }
}