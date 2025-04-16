using PluginInterface;
using System.Drawing;

namespace FramePlugin
{
    [Version(1, 0)]
    public class FrameTransform : IPlugin
    {
        public string Name => "Художественная рамка";
        public string Author => "vlpogudin";

        public void Transform(Bitmap bitmap)
        {
            int borderWidth = 20;
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                using (Pen pen = new Pen(Color.Red, borderWidth))
                {
                    g.DrawRectangle(pen, 0, 0, bitmap.Width - 1, bitmap.Height - 1);
                }
            }
        }
    }
}
