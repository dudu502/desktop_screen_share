using Main.Ext;
using Think.Viewer;

namespace Main
{
    public class Global
    {
        public static Setting setting = new Setting();
        public static string ServerIP = string.Empty;
        public static string HostName = string.Empty;   
    }
    
    internal class Program
    {
        static string key = "Host";
        static void Main(string[] args)
        {
            MainServerApp.Logger = new ConsoleLogger(key, string.Empty);
            MainServerApp app = new MainServerApp(key);
            app.StartServer(Global.setting.HostPort);
            Console.ReadKey();
        }
    }
}