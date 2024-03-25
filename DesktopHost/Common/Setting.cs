
namespace Think.Viewer.Common
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
        public int MultiThreadCount;
        public override string ToString()
        {
            return $"HostPort:{HostPort} StreamingPort:{StreamingPort} Capture:{CaptureX} {CaptureY} {CaptureWidth} {CaptureHeight} StreamingQuality:{StreamingQuality} Multi-Thread:{MultiThreadCount}";
        }
    }
}
