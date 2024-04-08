
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Think.Viewer.Common;
using UnityEngine;
using UnityEngine.UI;


public class ImageLoader : MonoBehaviour
{
    [SerializeField] RawImage rawImg;
    [SerializeField] TextAsset textAsset;

    TcpClient tcpClient;
    ConcurrentQueue<byte[]> bufferFrameQueue = new ConcurrentQueue<byte[]>();
    Thread thread;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        rawImg.texture = new Texture2D(2,2, TextureFormat.ARGB32, false);
        tcpClient = new TcpClient();
        ConnectAsync();
        thread = new Thread(ReceiveImages);
        thread.Start();
    }

    byte[] intBytes = new byte[4];
    byte[] buffer = new byte[Const.BUFFER_SIZE];
    void ReceiveImages()
    {
        while (true)
        {
            if (tcpClient.Connected && connected)
            {
                var stream = tcpClient.GetStream();
                if (stream.CanRead)
                {
                    Debug.LogWarning("Frame Start Read FullFrame [");
                    byte[] frameRaw = null;
                    stream.Read(intBytes, 0, 4);
                    int totalLen = BitConverter.ToInt32(intBytes);
                    Debug.LogWarning("Frame Totoal Size "+totalLen);
                    int bytesRead = 0;
                    while (bytesRead < totalLen)
                    {
                        Debug.LogWarning("Frame Start Read seg [ " + bytesRead);
                        int thisread = stream.Read(buffer, 0, Math.Min(buffer.Length, totalLen - bytesRead));
                        if (frameRaw == null)
                            frameRaw = new byte[0];
                        Debug.LogWarning("Frame Start Read seg [[ Resize to " + (frameRaw.Length + thisread));
                        Array.Resize(ref frameRaw, frameRaw.Length + thisread);
                        Debug.LogWarning("Frame Start Read seg [[ Resized to " + (frameRaw.Length) + $" CopyOp bufferSize:{buffer.Length} frameRawSize:{frameRaw.Length} dstOffset:{0+bytesRead} count:{thisread}");
                        Buffer.BlockCopy(buffer, 0, frameRaw, 0 + bytesRead, thisread);
                        Debug.LogWarning("Frame Start Read seg [[ BlockCopy " + frameRaw.Length);
                        bytesRead += thisread;
                        Debug.LogWarning("Frame Start Read seg ] "+ bytesRead);
                    }
                    bufferFrameQueue.Enqueue(frameRaw);
                    Debug.LogWarning("Frame Start Read FullFrame ]");
                }
            }
        }
    }

    bool connected;
    async void ConnectAsync()
    {
        await tcpClient.ConnectAsync("192.168.50.236", 8000);
        Debug.LogWarning("ConnectAsync !");
        connected=true;
    }


    void Update()
    {
        if(bufferFrameQueue.TryDequeue(out var frame))
        {
            ((Texture2D)rawImg.texture).LoadImage(frame, true);
            Debug.LogWarning("Frame Update Display Image " + frame.Length);
        }
        Debug.LogWarning("Frame Update remain count "+bufferFrameQueue.Count);
    }
}
