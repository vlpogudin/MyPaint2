using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace Lab_1
{
    /// <summary>
    /// Форма документа
    /// </summary>
    public partial class FormNewDocument : Form
    {
        #region Поля формы документа

        /// <summary>
        /// Координата на холсте перед выполнением действия
        /// </summary>
        private int oldX, oldY;

        /// <summary>
        /// Холст с изображением
        /// </summary>
        public Bitmap bitmap;

        /// <summary>
        /// Временный холст с изображением
        /// </summary>
        private Bitmap bitmapTemp;

        /// <summary>
        /// Текущий масштаб холста
        /// </summary>
        private float scale = 1.0f;

        /// <summary>
        /// Флаг для отслеживания рисования многоугольника
        /// </summary>
        private bool isDrawingPolygon = false; 

        /// <summary>
        /// Начальная точка рисования правильного n-угольника
        /// </summary>
        private Point polygonStart;

        /// <summary>
        /// Конечная точка рисования правильного n-угольника
        /// </summary>
        private Point polygonEnd;

        /// <summary>
        /// Флаг для отслеживания рисования линии
        /// </summary>
        private bool isDrawingLine = false;

        /// <summary>
        /// Начальная точка рисования линии
        /// </summary>
        private Point lineStart;

        /// <summary>
        /// Конечная точка рисования линии
        /// </summary>
        private Point lineEnd;

        #endregion

        #region Свойства формы документа

        /// <summary>
        /// Флаг, показывающий сохранен/не сохранен документ
        /// </summary>
        public bool IsSaved { get; set; } = false;
        
        /// <summary>
        /// Флаг, показывающий изменен/не изменен документ
        /// </summary>
        public bool IsModified {  get; set; } = false;

        /// <summary>
        /// Текущий путь файла
        /// </summary>
        public string FilePath { get; set; }

        #endregion

        #region Конструкторы формы документа

        /// <summary>
        /// Инициализация формы документа по умолчанию
        /// </summary>
        public FormNewDocument()
        {
            InitializeComponent();
            bitmap = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
            }
            bitmapTemp = bitmap;
            Resize += FormNewDocument_Resize;
        }

        /// <summary>
        /// Инициализация формы документа по переданному пути изображения
        /// </summary>
        /// <param name="imagePath"></param>
        public FormNewDocument(string imagePath)
        {
            InitializeComponent();
            bitmap = new Bitmap(imagePath);
            bitmapTemp = bitmap;
            ClientSize = new Size(bitmap.Width, bitmap.Height);
            Invalidate();
            Resize += FormNewDocument_Resize;
        }

        #endregion

        #region Движения мыши

        /// <summary>
        /// Обработка нажатия кнопки мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormNewDocument_MouseDown(object sender, MouseEventArgs e)
        {
            if (MainForm.CurrentTool == Tools.Text)
            {
                string text = Interaction.InputBox("Введите текст:", "Текст", "");
                if (!string.IsNullOrEmpty(text))
                {
                    // Печатаем текст на холсте
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        var font = new Font("Tahoma", 14);
                        var brush = new SolidBrush(MainForm.CurrentColor);
                        g.DrawString(text, font, brush, e.Location);
                    }
                    Invalidate();
                }
            }
            else if (MainForm.CurrentTool == Tools.BucketFill)
            {
                Color targetColor = bitmap.GetPixel(e.X, e.Y); // Получаем цвет пикселя, на который нажали
                FillArea(new Point(e.X, e.Y), targetColor, MainForm.CurrentColor); // Заливаем область
                Invalidate();
            }
            else if (MainForm.CurrentTool == Tools.RegularPolygon)
            {
                isDrawingPolygon = true; // Начинаем рисование многоугольника
                polygonStart = e.Location; // Запоминаем начальную точку
                polygonEnd = e.Location; // Инициализируем конечную точку
            }
            else if (MainForm.CurrentTool == Tools.Line)
            {
                isDrawingLine = true; // Начинаем рисование линии
                lineStart = e.Location; // Запоминаем начальную точку линии
                lineEnd = e.Location; // Инициализируем конечную точку
            }
            else
            {
                oldX = e.X; // Сохраняем координаты
                oldY = e.Y;
            }
        }

        /// <summary>
        /// Обработка движения указателя мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormNewDocument_MouseMove(object sender, MouseEventArgs e)
        {
            // Преобразуем координаты курсора с учетом масштаба, чтобы точки для рисования стали корректны
            var transform = new System.Drawing.Drawing2D.Matrix(); // Создаем пустую матрицу
            transform.Scale(scale, scale); // Умножаем координаты на масштаб
            transform.Invert(); // Инвертируем матрицу - получаем начальные координаты точек
            var points = new Point[] { e.Location }; // Создаем массив точек с текущими координатами
            transform.TransformPoints(points); // Перевод координаты мыши из масштабированных в реальные
            var transformedPoint = points[0]; // Координаты без влияния масштаба
            if (e.Button == MouseButtons.Left)
            {
                IsModified = true; // Устанавливаем флаг изменений
                var pen = new Pen(MainForm.CurrentColor, MainForm.CurrentWidth); // Кисть для рисования
                var brush = new SolidBrush(MainForm.CurrentColor); // Кисть для заливки
                // Обработка, чтобы кисти рисовали без пустых промежутков
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                // Преобразуем старые координаты, чтобы не было смещения
                var oldPoints = new Point[] { new Point(oldX, oldY) };
                transform.TransformPoints(oldPoints);
                var transformedOldPoint = oldPoints[0]; // Координаты без влияния масштаба
                switch (MainForm.CurrentTool)
                {
                    // Инструмент кисть
                    case Tools.Pen: 
                        var g = Graphics.FromImage(bitmap);
                        g.DrawLine(pen, transformedOldPoint.X, transformedOldPoint.Y, transformedPoint.X, transformedPoint.Y);
                        oldX = e.X; // Сохраняем текущие координаты курсора
                        oldY = e.Y;
                        bitmapTemp = bitmap;
                        Invalidate();
                        break;

                    // Инструмент окружность
                    case Tools.Circle:
                        bitmapTemp = (Bitmap)bitmap.Clone();
                        g = Graphics.FromImage(bitmapTemp);
                        var rectCircle = new Rectangle(transformedOldPoint.X, transformedOldPoint.Y, transformedPoint.X - transformedOldPoint.X, transformedPoint.Y - transformedOldPoint.Y);
                        if (MainForm.IsFilled)
                        {
                            g.FillEllipse(brush, rectCircle); // Заливаем окружность
                        }
                        else
                        {
                            g.DrawEllipse(pen, rectCircle); // Рисуем незакрашенную окружность
                        }
                        Invalidate();
                    break;

                    // Инструмент прямоугольник
                    case Tools.Rectangle:
                        bitmapTemp = (Bitmap)bitmap.Clone();
                        g = Graphics.FromImage(bitmapTemp);
                        var rect = new Rectangle(transformedOldPoint.X, transformedOldPoint.Y, transformedPoint.X - transformedOldPoint.X, transformedPoint.Y - transformedOldPoint.Y);
                        if (MainForm.IsFilled)
                        {
                            g.FillRectangle(brush, rect); // Заливаем прямоугольник
                        }
                        else
                        {
                            g.DrawRectangle(pen, rect); // Рисуем незакрашенный прямоугольник
                        }
                        Invalidate();
                    break;

                    // Инструмент ластик
                    case Tools.Eraser:
                        var eraserPen = new Pen(Color.White, MainForm.CurrentWidth);
                        eraserPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                        eraserPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                        g = Graphics.FromImage(bitmap);
                        g.DrawLine(eraserPen, transformedOldPoint.X, transformedOldPoint.Y, transformedPoint.X, transformedPoint.Y);
                        oldX = e.X; // Сохраняем текущие координаты курсора
                        oldY = e.Y;
                        bitmapTemp = bitmap;
                        Invalidate();
                    break;

                    // Инструмент паравильный n-угольник
                    case Tools.RegularPolygon:
                        if (isDrawingPolygon)
                        {
                            polygonEnd = e.Location;
                            Invalidate(); // Перерисовываем форму
                        }
                    break;

                    case Tools.Line:
                        if (isDrawingLine)
                        {
                            bitmapTemp = (Bitmap)bitmap.Clone(); // Создаем временное изображение
                            g = Graphics.FromImage(bitmapTemp);
                            g.DrawLine(pen, lineStart, transformedPoint); // Рисуем линию на временном изображении
                            lineEnd = transformedPoint; // Обновляем конечную точку
                            Invalidate(); // Перерисовываем экран
                        }
                    break;
                }
            }

            var parent = MdiParent as MainForm;
            parent?.ShowPosition(e.X, e.Y); // Обновляем координаты курсора в строке состояния
        }

        /// <summary>
        /// Обработка отпускания кнопки мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormNewDocument_MouseUp(object sender, MouseEventArgs e)
        {
            switch (MainForm.CurrentTool)
            {
                case Tools.Circle:
                case Tools.Rectangle:
                case Tools.Eraser:
                    bitmap = bitmapTemp;
                    Invalidate();
                    break;
                case Tools.RegularPolygon:
                    if (isDrawingPolygon)
                    {
                        isDrawingPolygon = false; // Завершаем рисование многоугольника
                        var polygonSidesForm = new FormPolygonSides(); // Диалоговое окно количества сторон
                        if (polygonSidesForm.ShowDialog() == DialogResult.OK)
                        {
                            int sides = polygonSidesForm.NumberOfSides;
                            // Вычисляем радиус многоугольника
                            int radius = (int)Math.Sqrt(Math.Pow(polygonEnd.X - polygonStart.X, 2) + Math.Pow(polygonEnd.Y - polygonStart.Y, 2));
                            // Вычисляем вершины многоугольника
                            var vertices = CalculatePolygonVertices(polygonStart, radius, sides);
                            // Рисуем многоугольник
                            using (var g = Graphics.FromImage(bitmap))
                            {
                                if (MainForm.IsFilled)
                                {
                                    g.FillPolygon(new SolidBrush(MainForm.CurrentColor), vertices); // Заливаем многоугольник
                                }
                                else
                                {
                                    g.DrawPolygon(new Pen(MainForm.CurrentColor, MainForm.CurrentWidth), vertices); // Рисуем незакрашенный многоугольник
                                }
                            }
                            Invalidate();
                        }
                    }
                break;

                case Tools.Line:
                    {
                        if (isDrawingLine)
                        {
                            isDrawingLine = false;
                            var g = Graphics.FromImage(bitmap);
                            g.DrawLine(new Pen(MainForm.CurrentColor, MainForm.CurrentWidth), lineStart, lineEnd);
                            bitmapTemp = bitmap; // Сохраняем нарисованную линию
                            Invalidate();
                        }
                    }
                break;
            }
        }

        /// <summary>
        /// Обработка покидания курсора формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormNewDocument_MouseLeave(object sender, EventArgs e)
        {
            var parent = MdiParent as MainForm;
            parent?.ShowPosition(-1, -1);
        }

        #endregion

        #region Действия над холстом

        /// <summary>
        /// Обработка рисования на холсте
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (bitmapTemp == null || this.ClientSize.Width <= 0 || this.ClientSize.Height <= 0)
                return;
            e.Graphics.ScaleTransform(scale, scale); // Применяем масштаб к изображению
            e.Graphics.DrawImage(bitmapTemp, 0, 0); // Отрисовываем изображение
            // Предварительный просмотр многоугольника
            if (isDrawingPolygon && MainForm.CurrentTool == Tools.RegularPolygon)
            {
                // Вычисляем радиус многоугольника
                int radius = (int)Math.Sqrt(Math.Pow(polygonEnd.X - polygonStart.X, 2) + Math.Pow(polygonEnd.Y - polygonStart.Y, 2));
                // Вычисляем вершины многоугольника
                var vertices = CalculatePolygonVertices(polygonStart, radius, 6); // По умолчанию 6 сторон для предварительного просмотра
                // Рисуем предварительный многоульник
                using (var pen = new Pen(Color.Gray, 1))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash; // Пунктир
                    e.Graphics.DrawPolygon(pen, vertices);
                }
            }
        }

        /// <summary>
        /// Обработка изменения размера холста
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // Проверяем, что окно не свернуто и размеры корректны
            if (this.WindowState == FormWindowState.Minimized || this.ClientSize.Width <= 0 || this.ClientSize.Height <= 0)
                return;
            if (bitmap != null && (bitmap.Width != this.ClientSize.Width || bitmap.Height != this.ClientSize.Height))
            {
                Bitmap newBitmap = new Bitmap(this.ClientSize.Width, this.ClientSize.Height); // Создаем новый Bitmap с новыми размерами
                using (var g = Graphics.FromImage(newBitmap))
                {
                    g.Clear(Color.White); // Очищаем новый Bitmap
                    g.DrawImage(bitmap, 0, 0); // Копируем старое изображение на новый Bitmap
                }
                // Сохраняем новый Bitmap
                bitmap = newBitmap;
                bitmapTemp = bitmap; // Обновляем bitmapTemp
            }
            Invalidate(); // Перерисовываем форму
        }

        /// <summary>
        /// Обработка закрытия формы
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (IsModified) // Если файл был изменен
            {
                // Запрашиваем у пользователя подтверждение
                var result = MessageBox.Show(
                    "Сохранить изменения перед закрытием?",
                    "Сохранение",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question
                );
                if (result == DialogResult.Yes)
                {
                    var mainForm = this.MdiParent as MainForm;
                    mainForm?.сохранитьToolStripMenuItem_Click(null, null); // Сохраняем файл
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Обработка прокрутки колесика мыши
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta > 0) // Увеличиваем масштаб
                scale *= 1.1f; // Увеличение на 10%
            else // Уменьшаем масштаб
                scale /= 1.1f; // Уменьшение на 10%
            scale = Math.Max(0.1f, Math.Min(3.0f, scale));  // Ограничиваем масштаб

            var parent = MdiParent as MainForm;
            parent?.ShowScale(scale); // Показываем масштаб в строке состояния
            Invalidate();
        }

        /// <summary>
        /// Сохранение файла по указанному пути указанным форматом
        /// </summary>
        /// <param name="path">Путь для сохранения документа</param>
        /// <param name="format">Формат документа</param>
        public void SaveAs(string path, ImageFormat format)
        {
            bitmap.Save(path, format);
            this.FilePath = path; // Сохраняем путь к файлу
            this.IsSaved = true; // Устанавливаем флаг сохранения
            this.IsModified = false; // Сбрасываем флаг изменений
        }

        /// <summary>
        /// Обработка изменения размеров холста
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormNewDocument_Resize(object sender, EventArgs e)
        {
            var parent = MdiParent as MainForm;
            if (parent != null)
                parent.ShowSize(this.ClientSize.Width, this.ClientSize.Height); // Обновляем размеры в статусной строке
        }

        /// <summary>
        /// Обработка изменения размеров холста через спец. окно
        /// </summary>
        /// <param name="newWidth">Новая ширина холста</param>
        /// <param name="newHeight">Новая высота холста</param>
        public void ResizeCanvas(int newWidth, int newHeight)
        {
            Bitmap newBitmap = new Bitmap(newWidth, newHeight); // Создаем новый Bitmap с новыми размерами
            using (var g = Graphics.FromImage(newBitmap))
            {
                g.Clear(Color.White);
                g.DrawImage(bitmap, 0, 0); // Копируем старое изображение на новый холст
            }
            // Обновляем Bitmap и размеры формы
            bitmap = newBitmap;
            bitmapTemp = bitmap;
            this.ClientSize = new Size(newWidth, newHeight);
            Invalidate();
        }

        /// <summary>
        /// Заливка области цветом
        /// </summary>
        /// <param name="point">Точка на холсте, куда произошло нажатие</param>
        /// <param name="targetColor">Цвет в точке, в которую произошло нажатие</param>
        /// <param name="replacementColor">Текущий цвет кисти, которым будет залита область</param>
        private void FillArea(Point point, Color targetColor, Color replacementColor)
        {
            // Если цвет под точкой и текущий одинаковые, изменений не происходит
            if (targetColor.ToArgb() == replacementColor.ToArgb())
                return;

            var queue = new Queue<Point>(); // Создание очереди точек
            queue.Enqueue(point); // Стартовую позицию добавляем в очередь
            // Идем по очереди, пока она не пуста
            while (queue.Count > 0)
            {
                var current = queue.Dequeue(); // Достаем текущую точку
                // Проверяем, что точка находится в границах изображения
                if (current.X < 0 || current.X >= bitmap.Width || current.Y < 0 || current.Y >= bitmap.Height)
                    continue;
                var pixelColor = bitmap.GetPixel(current.X, current.Y); // Получаем цвет пикселя в данной точке
                // Проверяем, что цвет пикселя и цвет заливки различны
                if (pixelColor.ToArgb() != targetColor.ToArgb())
                    continue;
                // Меняем цвет текущего пикселя на текущий цвет кисти
                bitmap.SetPixel(current.X, current.Y, replacementColor);
                // Добавляем пиксели, расположенные вокруг оббработанной точки
                queue.Enqueue(new Point(current.X + 1, current.Y));
                queue.Enqueue(new Point(current.X - 1, current.Y));
                queue.Enqueue(new Point(current.X, current.Y + 1));
                queue.Enqueue(new Point(current.X, current.Y - 1));
            }
        }

        /// <summary>
        /// Вычисление координат вершин правильного n-угольника
        /// </summary>
        /// <param name="center">Координата центра фигуры</param>
        /// <param name="radius">Радиус фигуры</param>
        /// <param name="sides">Количество сторон фигуры</param>
        /// <returns></returns>
        private Point[] CalculatePolygonVertices(Point center, int radius, int sides)
        {
            var points = new Point[sides]; // Массив координат всех вершин n-угольника
            double angle = 2 * Math.PI / sides; // Угол между вершинами
            // Вычисляем координаты для каждой точки
            for (int i = 0; i < sides; i++)
            {
                points[i] = new Point(
                    center.X + (int)(radius * Math.Cos(i * angle)),
                    center.Y + (int)(radius * Math.Sin(i * angle))
                );
            }
            return points;
        }

        #endregion
    }
}