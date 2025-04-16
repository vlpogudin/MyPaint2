using PluginInterface;
using System.Drawing;

namespace NegativePlugin
{
    [Version(1, 0)]
    public class NegativeTransform : IPlugin
    {
        public string Name => "Негатив";
        public string Author => "vlpogudin";

        public void Transform(Bitmap bitmap)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    Color newColor = Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B);
                    bitmap.SetPixel(x, y, newColor);
                }
            }
        }
    }
}
