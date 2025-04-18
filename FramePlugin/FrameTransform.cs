using PluginInterface;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing.Drawing2D;

namespace FramePlugin
{
    [Version(1, 0)]
    public class FrameTransform : IPlugin
    {
        #region Свойства

        /// <summary>
        /// Название плагина
        /// </summary>
        public string Name => "Художественная рамка";

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
            if (bitmap.Width < 20 || bitmap.Height < 20)
                throw new ArgumentException("Изображение слишком маленькое для рамки.");

            int borderWidth = 20;
            await Task.Run(async () =>
            {
                // Проверяем, не нажали ли "Отмена"
                cancellationToken.ThrowIfCancellationRequested();

                // Создаем объект Graphics для рисования
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    // Делаем линии гладкими
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    // Этап 1: Рисуем градиентную рамку (синий → зеленый)
                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        Color.Blue, // Начальный цвет
                        Color.Green, // Конечный цвет
                        LinearGradientMode.ForwardDiagonal)) // Диагональный градиент
                    {
                        using (Pen pen = new Pen(brush, borderWidth))
                        {
                            // Рисуем рамку, отступая от краев
                            g.DrawRectangle(pen, borderWidth / 2, borderWidth / 2,
                                bitmap.Width - borderWidth, bitmap.Height - borderWidth);
                        }
                    }
                    // Сообщаем 50% прогресса
                    progress?.Report(50);
                    await Task.Delay(500, cancellationToken);
                    using (Pen innerPen = new Pen(Color.White, 2))
                    {
                        // Линия ближе к центру
                        g.DrawRectangle(innerPen, borderWidth, borderWidth,
                            bitmap.Width - 2 * borderWidth, bitmap.Height - 2 * borderWidth);
                    }
                    // Сообщаем 100% прогресса
                    progress?.Report(100);
                    // Последняя задержка
                    await Task.Delay(500, cancellationToken);
                }
            }, cancellationToken);
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