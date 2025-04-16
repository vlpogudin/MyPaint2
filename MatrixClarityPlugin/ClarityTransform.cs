using PluginInterface;
using System;
using System.Drawing;

namespace MatrixClarityPlugin
{
    [Version(1, 0)]
    public class ClarityTransform : IPlugin
    {
        public string Name => "Улучшение четкости";
        public string Author => "vlpogudin";

        private readonly float[,] kernel = new float[,]
        {
            { -1, -1, -1 },
            { -1,  9, -1 },
            { -1, -1, -1 }
        };

        public void Transform(Bitmap bitmap)
        {
            Bitmap result = new Bitmap(bitmap.Width, bitmap.Height);
            for (int x = 1; x < bitmap.Width - 1; x++)
            {
                for (int y = 1; y < bitmap.Height - 1; y++)
                {
                    float r = 0, g = 0, b = 0;
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        for (int ky = -1; ky <= 1; ky++)
                        {
                            Color pixel = bitmap.GetPixel(x + kx, y + ky);
                            float weight = kernel[kx + 1, ky + 1];
                            r += pixel.R * weight;
                            g += pixel.G * weight;
                            b += pixel.B * weight;
                        }
                    }
                    r = Math.Min(Math.Max(r, 0), 255);
                    g = Math.Min(Math.Max(g, 0), 255);
                    b = Math.Min(Math.Max(b, 0), 255);
                    result.SetPixel(x, y, Color.FromArgb((int)r, (int)g, (int)b));
                }
            }
            // Копируем результат в исходный bitmap
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(result, 0, 0);
            }
        }
    }
}
