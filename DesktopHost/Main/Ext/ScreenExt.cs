using SevenZip.Compression.LZMA;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
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
        public static void Init(Rectangle screenRect,long quality = 20L)
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
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }
            return null;
        }
    }
}
