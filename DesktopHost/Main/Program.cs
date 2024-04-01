using Common;
using Main.Ext;
using NAudio.Wave;
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
            //Test();
        }

        static void Test()
        {
            var capture = new WasapiLoopbackCapture();
            capture.DataAvailable += Capture_DataAvailable;
            capture.StartRecording();
            Console.ReadKey();
            ms.Flush();
            ms.SetLength(0);
            Console.ReadKey();
            capture.StopRecording();
        }
        static MemoryStream ms = new MemoryStream();
        private static void Capture_DataAvailable(object? sender, WaveInEventArgs e)
        {
            ms.Write(e.Buffer, 0, e.BytesRecorded);
            Console.WriteLine($"bufferSize:{e.Buffer.Length} recorded size:{e.BytesRecorded} ms.Len:{ms.Length} ms.pos:{ms.Position}");
        }
    }
}