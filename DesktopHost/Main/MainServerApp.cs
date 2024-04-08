using Development.Net.Pt;
using Main.Ext;
using Main.Modules.Host;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Think.Viewer.Common;
using Think.Viewer.Core;
using Think.Viewer.Event;
using Think.Viewer.Modules;

namespace Main
{
    public class MainServerApp : BaseApplication
    {
        public MainServerApp(string key) : base(key)
        {

        }

        public override void StartServer(int port)
        {
            base.StartServer(port);
            manager.MessageReceived += OnMessageReceive;
        }

        private void OnMessageReceive(PtMessagePackage obj)
        {
            Logger.Log($"OnMessageReceive pid:{obj.MessageId} fromIp:{new IPAddress(obj.FromIp)} fromPort:{obj.FromPort}");
            EventDispatcher<C2S, PtMessagePackage>.DispatchEvent((C2S)obj.MessageId, obj);
        }

        protected override void SetUp()
        {
            base.SetUp();
            SetUpConfigs();
            AddModule(new CaptureHostModule(this));
            AddModule(new RequestProcessModule(this));
        }
        void SetUpConfigs()
        {
            string settingsPath = Path.Combine(Environment.CurrentDirectory, "setting.json");
            if (!File.Exists(settingsPath))
            {
                Global.setting = new Setting();
                File.WriteAllText(settingsPath,LitJson.JsonMapper.ToJson(Global.setting));
            }
            else
            {
                Global.setting = LitJson.JsonMapper.ToObject<Setting>(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "setting.json")));
            }
 
            Logger.Log(Global.setting.ToString());

            var hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            foreach (IPAddress ip in ipEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Global.ServerIP = ip.ToString();
                }
            }
            Global.HostName = hostName;
            Rectangle rectangle = new Rectangle(Global.setting.CaptureX, Global.setting.CaptureY, Global.setting.CaptureWidth, Global.setting.CaptureHeight);
            ScreenExt.Init(rectangle, (long)Convert.ToInt32(Global.setting.StreamingQuality));
        }

       
    }
}
