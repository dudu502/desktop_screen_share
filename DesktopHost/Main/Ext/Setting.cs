using System.Drawing;

namespace Main.Ext
{
    public class Setting
    {
        public int HostPort;
        public int StreamingPort;
        public int CaptureX;
        public int CaptureY;
        public int CaptureWidth;
        public int CaptureHeight;
        public string StreamingQuality = "12";
        public override string ToString()
        {
            return $"HostPort:{HostPort} StreamingPort:{StreamingPort} Capture:{CaptureX} {CaptureY} {CaptureWidth} {CaptureHeight} StreamingQuality:{StreamingQuality}";
        }

        public Rectangle GetCaptureRectangle()
        {
            return new Rectangle(CaptureX, CaptureY, CaptureWidth, CaptureHeight);
        }
    }
}
