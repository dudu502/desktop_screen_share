using SevenZip.Compression.LZMA;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Main.Ext
{
    internal class ScreenExt
    {
        static Rectangle screenBounds = new Rectangle(0, 0, 2240, 1400);
        static Bitmap screenshot;
        static ImageCodecInfo jpegEncoder;
        static EncoderParameters encoderParameters = new EncoderParameters(1);
        static EncoderParameter encoderParameter;

        public static void ChangeQuality(int quality)
        {
            encoderParameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
            encoderParameters.Param[0] = encoderParameter;
        }
        public static void Init(Rectangle screenRect,long quality = 50L)
        {
            screenBounds = screenRect;
            screenshot = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppArgb);
            jpegEncoder = GetEncoder(ImageFormat.Jpeg);
            encoderParameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParameters.Param[0] = encoderParameter;
        }
        public static byte[] CaptureScreenBytes()
        {
            Graphics graphics = Graphics.FromImage(screenshot);
            graphics.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0, screenBounds.Size, CopyPixelOperation.SourceCopy);
            
            using (var ms = new MemoryStream())
            {
                screenshot.Save(ms, jpegEncoder, encoderParameters);
                var raw = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(raw, 0, raw.Length);
                return raw;
            }
        }
        static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }
            return null;
        }

        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);
        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);

        public static byte[] BitBltCaptureScreenBytes()
        {
            IntPtr desktopHandle = GetDesktopWindow();
            IntPtr desktopDC = GetWindowDC(desktopHandle);
            Size screenSize = new Size(screenBounds.Width, screenBounds.Height);
            Bitmap screenImage = new Bitmap(screenSize.Width, screenSize.Height);
            Graphics g = Graphics.FromImage(screenImage);

            IntPtr gHdc = g.GetHdc();
            BitBlt(gHdc, screenBounds.X, screenBounds.Y, screenSize.Width, screenSize.Height, desktopDC, 0, 0, 0x00CC0020); // SRCCOPY
            g.ReleaseHdc(gHdc);

            using (var ms = new MemoryStream())
            {
                screenImage.Save(ms, jpegEncoder, encoderParameters);
                var raw = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(raw, 0, raw.Length);
                return raw;
            }

        }


        public static byte[] ZipBitBltCaptureScreenBytes()
        {
            IntPtr desktopHandle = GetDesktopWindow();
            IntPtr desktopDC = GetWindowDC(desktopHandle);
            Size screenSize = new Size(screenBounds.Width, screenBounds.Height);
            Bitmap screenImage = new Bitmap(screenSize.Width, screenSize.Height);
            Graphics g = Graphics.FromImage(screenImage);

            IntPtr gHdc = g.GetHdc();
            BitBlt(gHdc, screenBounds.X, screenBounds.Y, screenSize.Width, screenSize.Height, desktopDC, 0, 0, 0x00CC0020); // SRCCOPY
            g.ReleaseHdc(gHdc);

            using (var ms = new MemoryStream())
            {
                screenImage.Save(ms, jpegEncoder, encoderParameters);
                //var raw = new byte[ms.Length];
                //ms.Seek(0, SeekOrigin.Begin);
                //ms.Read(raw, 0, raw.Length);

                using(Stream outStream = new MemoryStream())
                {
                    SevenZip.Helper.Compress(ms, outStream);
                    return SevenZip.Helper.StreamToByteArray(outStream);
                }
            }

        }

    }
}
