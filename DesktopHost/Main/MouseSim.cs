using Development.Net.Pt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Main
{
    public class MouseSim
    {
        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        const int MOUSEEVENTF_MOVE = 0x0001;
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        public static void MouseLeftClick(int x,int y)
        {
            Console.WriteLine($"MouseLeftClick {x} {y}");
            SetCursorPos(x,y);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)x, (uint)y, 0, 0);
        }
        public static void MouseLeftDown(int x, int y)
        {
            Console.WriteLine($"MouseLeftDown {x} {y}");
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_LEFTDOWN , (uint)x, (uint)y, 0, 0);
        }
        public static void MouseLeftUp(int x, int y)
        {
            Console.WriteLine($"MouseLeftUp {x} {y}");
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_LEFTUP, (uint)x, (uint)y, 0, 0);
        }
        public static void MouseRightClick(int x,int y)
        {
            Console.WriteLine($"MouseLeftClick {x} {y}");
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (uint)x, (uint)y, 0, 0);
        }

        public static void MouseLeftDoubleClick(int x,int y)
        {
            Console.WriteLine($"MouseDoubleLeftClick {x} {y}");
            SetCursorPos(x, y);
            for(int i=0;i<2;i++)
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)x, (uint)y, 0, 0);
            }
        }
    }
}
