using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
    internal class ScreenSim
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, int lParam);

        public const uint WM_SYSCOMMAND = 0x0112;
        public const uint SC_MONITORPOWER = 0xF170;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        public static void TurnOff()
        {
            SendMessage(GetConsoleWindow(), WM_SYSCOMMAND, SC_MONITORPOWER, 2);
        }
        public static void TurnOn() 
        {
            SendMessage(GetConsoleWindow(), WM_SYSCOMMAND, SC_MONITORPOWER, -1);
        }
    }
}
