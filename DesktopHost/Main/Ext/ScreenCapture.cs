using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using System.Drawing.Imaging;
using System.Threading;

namespace Main.Ext
{
    public class ScreenCapture
    {
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);
        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);

        public static Bitmap CaptureScreen()
        {
            IntPtr desktopHandle = GetDesktopWindow();
            IntPtr desktopDC = GetWindowDC(desktopHandle);
            Size screenSize = new Size(2240,1400);
            Bitmap screenImage = new Bitmap(screenSize.Width, screenSize.Height);
            Graphics g = Graphics.FromImage(screenImage);

            IntPtr gHdc = g.GetHdc();
            BitBlt(gHdc, 0, 0, screenSize.Width, screenSize.Height, desktopDC, 0, 0, 0x00CC0020); // SRCCOPY
            g.ReleaseHdc(gHdc);

            return screenImage;
        }
        public static void SHARYDX()
        {
            
        }
    }
}
