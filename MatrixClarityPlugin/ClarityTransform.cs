using PluginInterface;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MatrixClarityPlugin
{
    [Version(1, 0)]
    public class ClarityTransform : IPlugin
    {
        #region Свойства

        /// <summary>
        /// Название плагина
        /// </summary>
        public string Name => "Улучшение четкости";

        /// <summary>
        /// Автор плагина
        /// </summary>
        public string Author => "vlpogudin";

        /// <summary>
        /// Матрица улучшения четкости (увеличивает разницу значений на границах)
        /// </summary>
        private readonly float[,] matrix = new float[,]
        {
            { -1, -1, -1 },
            { -1,  9, -1 },
            { -1, -1, -1 }
        };

        #endregion

        /// <summary>
        /// Асинхронный метод обработки  
        /// </summary>
        /// <param name="bitmap">Битовая карта изображения</param>
        /// <param name="progress">Прогресс выполнения</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns></returns>
        public async Task TransformAsync(Bitmap bitmap, IProgress<int> progress, CancellationToken cancellationToken)
        {
            // Проверяем, что изображение не пустое
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            int width = bitmap.Width;
            int height = bitmap.Height;
            int totalPixels = (width - 2) * (height - 2); // Вычисляем общее количество пикселей (края не обрабатываем)
            int processedPixels = 0; // Счетчик обработанных пикселей (для прогресса)
            int pixelsPerStage = totalPixels / 4;

            // Блокируем данные изображения
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);
            byte[] pixels; // Исходные пиксели
            byte[] resultPixels; // Обработанные пиксели
            int stride;

            try
            {
                stride = Math.Abs(bitmapData.Stride);
                pixels = new byte[stride * height];
                resultPixels = new byte[stride * height];
                // Копируем исходные данные
                Marshal.Copy(bitmapData.Scan0, pixels, 0, pixels.Length);

                // Разделяем обработку на потоки
                int numThreads = Environment.ProcessorCount;
                int rowsPerThread = (height - 2) / numThreads;
                Task[] tasks = new Task[numThreads]; // Создаем массив задач для параллельной обработки
                for (int t = 0; t < numThreads; t++)
                {
                    // Определяем начальную и конечную строку для текущего потока
                    int startRow = 1 + t * rowsPerThread;
                    int endRow = (t == numThreads - 1) ? height - 1 : startRow + rowsPerThread;

                    // Запускаем задачу для обработки строк
                    tasks[t] = Task.Run(() =>
                    {
                        for (int y = startRow; y < endRow; y++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            for (int x = 1; x < width - 1; x++) // Пропускаем края (вылезала ошибка)
                            {
                                float r = 0, g = 0, b = 0;
                                for (int ky = -1; ky <= 1; ky++)
                                {
                                    for (int kx = -1; kx <= 1; kx++)
                                    {
                                        // Вычисляем индекс соседнего пикселя
                                        int pixelIndex = (y + ky) * stride + (x + kx) * 3;
                                        float weight = matrix[kx + 1, ky + 1];
                                        // Умножаем значения цветов на вес
                                        b += pixels[pixelIndex] * weight;
                                        g += pixels[pixelIndex + 1] * weight;
                                        r += pixels[pixelIndex + 2] * weight;
                                    }
                                }
                                r = Math.Min(Math.Max(r, 0), 255);
                                g = Math.Min(Math.Max(g, 0), 255);
                                b = Math.Min(Math.Max(b, 0), 255);
                                int resultIndex = y * stride + x * 3; // Сохраняем результат
                                resultPixels[resultIndex] = (byte)b;
                                resultPixels[resultIndex + 1] = (byte)g;
                                resultPixels[resultIndex + 2] = (byte)r;
                            }
                            int currentPixels = Interlocked.Add(ref processedPixels, width - 2);
                            if (currentPixels / pixelsPerStage > 0 && currentPixels / pixelsPerStage <= 4)
                            {
                                int progressPercent = (currentPixels / pixelsPerStage) * 25;
                                progress?.Report(Math.Min(progressPercent, 100));
                                Task.Delay(50, cancellationToken).GetAwaiter().GetResult();
                            }
                        }
                    }, cancellationToken);
                }

                await Task.WhenAll(tasks);  // Ждем пока завершатся все задачи
                Marshal.Copy(resultPixels, 0, bitmapData.Scan0, resultPixels.Length); // Копируем результат обратно
            }
            finally
            {
                bitmap.UnlockBits(bitmapData); // Убираем блокировку с данных изображения
            }
        }

        /// <summary>
        /// Синхронный метод для обратной совместимости с интерфейсом IPlugin
        /// </summary>
        /// <param name="bitmap">Битовая карта изображения</param>
        public void Transform(Bitmap bitmap)
        {
            // Вызываем асинхронный метод синхронно, без прогресса и отмены
            TransformAsync(bitmap, null, CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}