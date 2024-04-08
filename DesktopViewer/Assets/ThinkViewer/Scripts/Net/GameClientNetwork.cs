using UnityEngine;
using System.Net;
using System.Collections.Concurrent;
using System;
using Think.Viewer.Common;
using Think.Viewer.Event;
using Think.Viewer.Manager;
using Think.Viewer.Module;
using Development.Net.Pt;
using static UnityEngine.Rendering.ReloadAttribute;

namespace Think.Viewer.Net
{
    public class GameClientNetwork
    {
        static GameClientNetwork _Inst = null;
        private UDPManager _client;
        public byte[] localIp;
        public int localPort = 50000;
        public static GameClientNetwork Instance
        {
            get
            {
                if (_Inst == null)
                    _Inst = new GameClientNetwork();
                return _Inst;
            }
        }

        private ConcurrentQueue<PtMessagePackage> m_QueueMsg;
        private GameClientNetwork()
        {
            m_QueueMsg = new ConcurrentQueue<PtMessagePackage>();
        }
        public void CloseClient()
        {

        }



        public void Start()
        {
            _client = new UDPManager(localPort);
            _client.Start();
            _client.MessageReceived += OnMessageReceived;
        }
      
        void OnMessageReceived(PtMessagePackage package)
        {
            EventDispatcher<S2C, PtMessagePackage>.DispatchEvent((S2C)package.MessageId, package);
        }


        public void SendUnconnectedRequestRaw(PtMessagePackage package)
        {
            if (_client != null)
            {
                _client.StartSendAsync(package);
            }
        }
        public void SendUnconnectedRequest(C2S pid, byte[] raw)
        {
            var currentRemoteEndPoint = ModuleManager.GetModule<DataModule>().CurrentEndPoint;
            if (currentRemoteEndPoint != null)
                SendUnconnectedRequestRaw(PtMessagePackage.Build((ushort)pid, raw)
                    .SetToIp(currentRemoteEndPoint.Address.GetAddressBytes())
                    .SetToPort(currentRemoteEndPoint.Port)
                    .SetFromIp(localIp)
                    .SetFromPort(localPort));
        }
        public void SendUnconnectedRequest(C2S pid, params object[] p)
        {
            var currentRemoteEndPoint = ModuleManager.GetModule<DataModule>().CurrentEndPoint;
            if (currentRemoteEndPoint != null)
                SendUnconnectedRequestRaw(PtMessagePackage.BuildParams((ushort)pid, p)
                    .SetToIp(currentRemoteEndPoint.Address.GetAddressBytes())
                    .SetToPort(currentRemoteEndPoint.Port)
                    .SetFromIp(localIp)
                    .SetFromPort(localPort));
        }

        public void TickDispatchMessages()
        {
            while (m_QueueMsg.Count > 0)
            {
                if (m_QueueMsg.TryDequeue(out PtMessagePackage package))
                {
                    try
                    {
                        EventDispatcher<S2C, PtMessagePackage>.DispatchEvent((S2C)package.MessageId, package);
                    }
                    catch (Exception exc)
                    {
                        Debug.LogError("TickDispatchMessages Error " + exc.ToString());
                    }
                }
            }
        }     
    }
}