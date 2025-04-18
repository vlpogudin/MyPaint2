using PluginInterface;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace NegativePlugin
{
    [Version(1, 0)]
    public class NegativeTransform : IPlugin
    {
        #region Свойства

        /// <summary>
        /// Название плагина
        /// </summary>
        public string Name => "Негатив";
        
        /// <summary>
        /// Автор плагина
        /// </summary>
        public string Author => "vlpogudin";

        #endregion

        /// <summary>
        /// Асинхронный метод обработки изображения
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
            int totalPixels = width * height; // Вычисляем общее количество пикселей
            int processedPixels = 0; // Счетчик обработанных пикселей (для прогресса)
            int pixelsPerStage = totalPixels / 2; ;

            // Блокируем данные изображения для прямого доступа к пикселям
            BitmapData bitmapData = bitmap.LockBits( // BitmapData хранит указатель на данные изображения и метаданные
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);
            try
            {
                int length = bitmapData.Stride; // Получаем длину строки в байтах
                byte[] pixels = new byte[Math.Abs(length) * height]; // Создаем массив для хранения всех пикселей изображения (B-G-R)
                Marshal.Copy(bitmapData.Scan0, pixels, 0, pixels.Length); // Копируем данные изображения из памяти в массив pixels
                int numThreads = Environment.ProcessorCount; // Определяем количество потоков на основе числа процессоров
                int rowsPerThread = height / numThreads; // Вычисляем количество строк изображения на один поток

                Task[] tasks = new Task[numThreads]; // Создаем массив задач для параллельной обработки
                for (int t = 0; t < numThreads; t++)
                {
                    // Определяем начальную и конечную строку для текущего потока
                    int startRow = t * rowsPerThread;
                    int endRow = (t == numThreads - 1) ? height : startRow + rowsPerThread;

                    // Запускаем задачу для обработки строк
                    tasks[t] = Task.Run(async () =>
                    {
                        for (int y = startRow; y < endRow; y++)
                        {
                            cancellationToken.ThrowIfCancellationRequested(); // Проверяем, не запрошена ли отмена операции
                                                                              // (оставил отмену, но кнопку отмены убрал - не работает)
                            // Перебираем пиксели в строке
                            for (int x = 0; x < width; x++)
                            {
                                int index = y * length + x * 3; // индекс первого байта пикселя (x, y), который соответствует каналу B
                                pixels[index] = (byte)(255 - pixels[index]);
                                pixels[index + 1] = (byte)(255 - pixels[index + 1]);
                                pixels[index + 2] = (byte)(255 - pixels[index + 2]);
                            }
                            int currentPixels = Interlocked.Add(ref processedPixels, width);
                            if (currentPixels >= pixelsPerStage * ((currentPixels / pixelsPerStage)) && (currentPixels / pixelsPerStage) <= 4)
                            {
                                int progressPercent = (int)((double)currentPixels / totalPixels * 100);
                                progress?.Report(Math.Min(progressPercent, 100));
                                await Task.Delay(50, cancellationToken);
                            }
                        }
                    }, cancellationToken); // Передаем токен отмены в задачу
                }

                await Task.WhenAll(tasks); // Ждем пока завершатся все задачи (т.к. копируем уже после обработки всех строк)
                Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length); // Копируем обработанные пиксели обратно в изображение
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