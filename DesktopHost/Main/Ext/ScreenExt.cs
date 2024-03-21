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
        public static byte[] CaptureScreenBytes()
        {
            Rectangle bounds = new Rectangle(0, 0, 2240, 1400);
            //Rectangle bounds = new Rectangle(0, 0, 2240/2, 1400/2);
            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(screenshot);
            graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);


            var jpegEncoder = GetEncoder(ImageFormat.Jpeg);
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters encoderParameters = new EncoderParameters(1);
            EncoderParameter encoderParameter = new EncoderParameter(myEncoder, 20L);
            encoderParameters.Param[0] = encoderParameter;


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
