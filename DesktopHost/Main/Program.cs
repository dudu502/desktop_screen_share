using Common;
using Main.Ext;
using System.Runtime.InteropServices;
using System.Text;
using Think.Viewer;
using Think.Viewer.Common;

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


            // Test();
            // Console.ReadKey();
        }

        async static void Test()
        {
            ScreenExt.Init(new System.Drawing.Rectangle(0, 0, 2240, 1400),50L);
            File.WriteAllBytes(Path.Combine(Environment.CurrentDirectory, "1.bytes"), await SevenZip.Helper.CompressBytesAsync(ScreenExt.BitBltCaptureScreenBytes()));
        }
    }
}