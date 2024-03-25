using Common;
using Main.Ext;
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


            //byte[] s = new byte[] { 1,2,32,4,4,5,6,6,7,8,89,99,0,2,2,2,2,25,5,66,88};
            //var ss= Utils.SplitByteArray(s,4);

            //foreach( var v in ss )
            //{
            //    Console.WriteLine(v);
            //}

        }
       

    }
}