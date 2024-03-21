using LiteNetLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Think.Viewer.Common;
using Think.Viewer.Core;
using Think.Viewer.Event;

namespace Think.Viewer.Modules
{
    public class HeartbeatModule: BaseModule
    {
        ConcurrentQueue<int> _queueDisconnectPeers = new ConcurrentQueue<int>();
        double _heartBeatDetectSecs = 0;
        Dictionary<int, DateTime> _dtPeerHeartBeat = new Dictionary<int, DateTime>(); 
        public HeartbeatModule(BaseApplication app,double heartBeatDetectSecs = 15):base(app)
        {
            _heartBeatDetectSecs = heartBeatDetectSecs;
            EventDispatcher<C2S, NetMessageEvt>.AddListener(C2S.Heartbeat, OnHeartBeat);
            EventDispatcher<NetActionEvent, NetPeer>.AddListener(NetActionEvent.PeerDisconnectedEvent, OnPeerDisconnectedEvent);
            EventDispatcher<NetActionEvent, object>.AddListener(NetActionEvent.CallCustomEvent, OnCallCustomEventAfterPollNetMessage);
            ThreadPool.QueueUserWorkItem(__HeartBeatRunningCheck,null);
        }

        public override void Dispose()
        {
            _queueDisconnectPeers = null;
            _dtPeerHeartBeat = null;
            base.Dispose();
        }
        void OnPeerDisconnectedEvent(NetPeer netPeer)
        {
            lock (_dtPeerHeartBeat)
            {
                _dtPeerHeartBeat.Remove(netPeer.Id);
                BaseApplication.Logger.Log("OnPeerDisconnectedEvent HeartbeatModule");               
            }
        }
        void OnCallCustomEventAfterPollNetMessage(object obj)
        {
            int peerId = -1;
            while(_queueDisconnectPeers.TryDequeue(out peerId))
            {
                BaseApplication.Logger.Log("Remove PeerId:" + peerId);
                var manager = GetNetManager();
                if(manager!=null)
                {
                    var peer = manager.GetPeerById(peerId);
                    if(peer!=null)
                        manager.DisconnectPeerForce(manager.GetPeerById(peerId));
                }
            }
        }

        void __HeartBeatRunningCheck(object obj)
        {
            for(; ; )
            {
                lock (_dtPeerHeartBeat)
                {
                    DateTime now = DateTime.Now;
                    List<int> peerIds = new List<int>(_dtPeerHeartBeat.Keys);
                    for (int i = peerIds.Count - 1; i > -1; --i)
                    {
                        int peerId = peerIds[i];
                        DateTime peerHeartBeatDt;
                        if(_dtPeerHeartBeat.TryGetValue(peerId, out peerHeartBeatDt))
                        {
                            TimeSpan span = now - peerHeartBeatDt;
                            if (span.TotalSeconds > _heartBeatDetectSecs)
                            {
                                _queueDisconnectPeers.Enqueue(peerId);
                                _dtPeerHeartBeat.Remove(peerId);
                            }
                        }                   
                    }
                    Thread.Sleep(500);
                }
                
            }
        }
        /// <summary>
        /// 心跳检测
        /// </summary>
        /// <param name="evt"></param>
        void OnHeartBeat(NetMessageEvt evt)
        {
            _dtPeerHeartBeat[evt.Peer.Id] = DateTime.Now;
        }
    }
}
