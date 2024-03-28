using System.Drawing.Imaging;

namespace DesktopHostForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        
            var bitmap = ScreenCapture.CaptureScreen();
            using(MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Jpeg);
                File.WriteAllBytes(Path.Combine(Environment.CurrentDirectory,"i.jpeg"),ms.ToArray());
            }
            
        }
    }
}