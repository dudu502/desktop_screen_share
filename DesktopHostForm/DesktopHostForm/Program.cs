using System.Runtime.InteropServices;

namespace DesktopHostForm
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
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
            Size screenSize = Screen.PrimaryScreen.Bounds.Size;
            Bitmap screenImage = new Bitmap(screenSize.Width, screenSize.Height);
            Graphics g = Graphics.FromImage(screenImage);

            IntPtr gHdc = g.GetHdc();
            BitBlt(gHdc, 0, 0, screenSize.Width, screenSize.Height, desktopDC, 0, 0, 0x00CC0020); // SRCCOPY
            g.ReleaseHdc(gHdc);

            return screenImage;
        }
    }
}