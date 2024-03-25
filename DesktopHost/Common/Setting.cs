
namespace Think.Viewer.Common
{
    public class Setting
    {
        public int HostPort = 7999;
        public int StreamingPort = 8000;
        public int CaptureX = 0;
        public int CaptureY = 0;
        public int CaptureWidth = 2240;
        public int CaptureHeight = 1400;
        public string StreamingQuality = "12";
        public int MultiThreadCount = 1;
        public override string ToString()
        {
            return $"HostPort:{HostPort} StreamingPort:{StreamingPort} Capture:{CaptureX} {CaptureY} {CaptureWidth} {CaptureHeight} StreamingQuality:{StreamingQuality} Multi-Thread:{MultiThreadCount}";
        }
    }
}
