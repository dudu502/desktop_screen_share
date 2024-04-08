using Development.Net.Pt;

using Think.Viewer.Common;
using Think.Viewer.Event;
using Think.Viewer.Modules;

namespace Think.Viewer.Core
{
    public enum NetActionEvent
    {
        CallCustomEvent,
        PeerDisconnectedEvent,
        PeerConnectedEvent,
        NetworkReceiveUnconnectedEvent,
        NetworkReceiveEvent,
        NetworkLatencyUpdateEvent,
        NetworkErrorEvent,
        DeliveryEvent,
        ConnectionRequestEvent,
    }

    public class BaseApplication
    {
        protected UDPManager manager;
        public int Port { private set; get; }
        protected string m_ApplicationKey;
        public static ILogger Logger;
        private Dictionary<string, object> m_ConfigMaps;
        public BaseApplication(string key)
        {
            m_ConfigMaps = new Dictionary<string, object>();
            m_ApplicationKey = key;
            Logger.Log(string.Format("Application [{0}] Initialize ApplicationKey:{1}.", GetType().ToString(), m_ApplicationKey));
            SetUp();
            Logger.Log("Application has Setup.");
        }
        public void AddConfigElement(string key, object data)
        {
            m_ConfigMaps[key] = data;
        }
        public T GetConfigElement<T>(string key)
        {
            if (m_ConfigMaps.ContainsKey(key))
            {
                return (T)m_ConfigMaps[key];
            }
            return default(T);
        }
        public string GetApplicationKey() { return m_ApplicationKey; }

        public void SendAsync(PtMessagePackage msg)
        {
            manager.StartSendAsync(msg);
        }
        public virtual void StartServer(int port)
        {
            Port = port;
            manager = new UDPManager(port);
            manager.Start();
            Logger.Log(string.Format("Server [{0}] has launched at port:{1} Success.", GetType().ToString(), port));
        }

        protected virtual void CallCustomEvent()
        {
            EventDispatcher<NetActionEvent, object>.DispatchEvent(NetActionEvent.CallCustomEvent, null);
        }
        public virtual void ShutDown()
        {
            Logger.Log("ShutDown");
            Logger = null;
        }
        protected virtual void SetUp()
        {

        }

        public virtual void Dispose()
        {
            m_ApplicationKey = null;
            m_ConfigMaps = null;
            BaseModule.RemoveAllModules();
        }

        protected virtual void AddModule(BaseModule module)
        {
            BaseModule.AddModule(module);
            Logger.Log(string.Format("AddModule [{0}] Success.", module.GetType().ToString()));
        }
    }

}