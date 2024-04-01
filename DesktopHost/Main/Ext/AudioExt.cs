using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Ext
{
    public class AudioExt
    {
        static bool Running = false;
        static MemoryStream ms = new MemoryStream();
        static WasapiLoopbackCapture audioCapture = new WasapiLoopbackCapture();
        static object SyncLock = new object();
        public static void TryStart()
        {
            if (!Running)
            {
                audioCapture.DataAvailable += OnDataAvailable;
                audioCapture.StartRecording();
                Running=true;
            }
        }
        static void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            lock(SyncLock)
                ms.Write(e.Buffer, 0,e.BytesRecorded);
        }
        public static byte[] CapturePeriodBytes()
        {
            lock (SyncLock)
            {
                var raw = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(raw, 0, raw.Length);
                ms.SetLength(0);
                return raw;
            }
        }
        public static void Stop()
        {
            if (Running)
            {
                audioCapture.DataAvailable -= OnDataAvailable;
                audioCapture.StopRecording();
            }
        }
    }
}
