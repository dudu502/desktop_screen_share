using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Think.Viewer.Common;
using Think.Viewer.Manager;
using ThinkViewer.Scripts.Net.Data;
using UnityEngine;
namespace Think.Viewer.Module
{
    public class MergedFrame
    {
        public byte[][] Result;
        public int Hash { private set; get; }
        public int SegCount { private set; get; }
        public byte[] FrameRaw;
        public MergedFrame(int hash,int segCount)
        {
            Hash = hash;
            SegCount = segCount;
            Result = new byte[segCount][];
        }

        public void TryMerge()
        {
            if (FrameRaw != null) return;
            for(int i = 0; i < SegCount; i++)
            {
                if (Result[i] == null)
                {
                    return;
                }
            }
            FrameRaw = Utils.CombineByteArrays(Result);
        }
    }
    public class DataModule : IModule
    {
        public Setting HostSetting;
        public List<HostNetInfo> HostNetInfos = new List<HostNetInfo>();
        public ConcurrentQueue<byte[]> StreamingRawFrameQueue = new ConcurrentQueue<byte[]>();
        internal IPEndPoint CurrentEndPoint;
        public object Sync = new object();
        public ConcurrentQueue< MergedFrame> MergedFrames = new ConcurrentQueue<MergedFrame>();
        public void Initialize()
        {
            
        }

        public void MergeSegFrame(int hash, int idx, byte[] raw)
        {
            Debug.LogError($"MergeSeg Frame hash:{hash} idx:{idx} raw:{raw}");
            bool exist=false;
            MergedFrame frame = null;
            foreach(var item in MergedFrames)
            {
                if (item.Hash == hash)
                {
                    exist = true;
                    frame = item;
                    break;
                }
            }
            if (!exist)
            {
                frame = new MergedFrame(hash, HostSetting.MultiThreadCount);
                MergedFrames.Enqueue(frame);
            }

            frame.Result[idx] = raw;
            frame.TryMerge();
  
            if(MergedFrames.TryPeek(out var f))
            {
                if (f.FrameRaw != null)
                {
                    MergedFrames.TryDequeue(out var t);
                    StreamingRawFrameQueue.Enqueue(f.FrameRaw);
                }
            }
            
        }
    }
}
