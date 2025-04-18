using PluginInterface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab_1
{
    /// <summary>
    /// Главная форма
    /// </summary>
    public partial class MainForm : Form
    {
        #region Поля главной формы

        /// <summary>
        /// Словарь плагинов (ключ - название плагина)
        /// </summary>
        Dictionary<string, IPlugin> plugins = new Dictionary<string, IPlugin>();

        /// <summary>
        /// Путь конфигурационного файла
        /// </summary>
        private const string ConfigFilePath = "plugins.txt";

        /// <summary>
        /// Токен для отмены операций
        /// </summary>
        private CancellationTokenSource cts;

        #endregion

        #region Свойства главной формы

        /// <summary>
        /// Текущий цвет кисти
        /// </summary>
        /// </summary>
        public static Color CurrentColor { get; set; }

        /// <summary>
        /// Текущая толщина кисти
        /// </summary>
        public static int CurrentWidth { get; set; }

        /// <summary>
        /// Текущий инструмент для рисования
        /// </summary>
        public static Tools CurrentTool { get; set; }

        /// <summary>
        /// Флаг, определяющий вкл/выкл заливка фигуры
        /// </summary>
        public static bool IsFilled { get; set; } = false;

        #endregion

        #region Конструкторы главной формы

        /// <summary>
        /// Инициализация главного окна
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            CurrentColor = Color.Black;
            CurrentWidth = 1;
            MdiChildActivate += MainForm_MdiChildActivate; // Подписываемся на событие активных окон
            LockToolbar(true); // Блокируем все кнопки панели инструментов

            FindPlugins(); // Поиск плагинов
            CreatePluginsMenu(); // Меню управления плагинами
        }

        #endregion

        #region Панель инструментов

        /// <summary>
        /// Открытие пункта меню "Файл"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void файлToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var doc = ActiveMdiChild as FormNewDocument; // Активное окно
            сохранитьКакToolStripMenuItem.Enabled = (doc != null);
            сохранитьToolStripMenuItem.Enabled = (doc != null);
            
        }

        /// <summary>
        /// Демонстрация окна "Новый документ"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void новыйДокументToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var doc = new FormNewDocument();
            doc.MdiParent = this; // Устаналиваем родительскую форму
            doc.Show();
            ShowScale(1);
            LockToolbar(false); // Разблокировка всех кнопок панели инструментов
        }

        /// <summary>
        /// Сохранение файла
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var doc = ActiveMdiChild as FormNewDocument; // Активное окно
            if (doc != null)
            {
                if (!doc.IsSaved) // Если док еще не был сохранен, сохраняем с указанием пути
                {
                    сохранитьКакToolStripMenuItem_Click(sender, e);
                }
                else // Если док уже был сохранен ранее
                {
                    ImageFormat format; // Сохраняем формат, далее сохраняем его с ним же по старому пути
                    switch (Path.GetExtension(doc.FilePath).ToLower()) 
                    {
                        case ".jpg":
                            format = ImageFormat.Jpeg;
                            break;
                        case ".bmp":
                            format = ImageFormat.Bmp;
                            break;
                        default:
                            throw new InvalidOperationException("Неподдерживаемый формат файла.");
                    }
                    doc.SaveAs(doc.FilePath, format); // Сохраняем по текущему имени файла
                }
            }
        }

        /// <summary>
        /// Сохранение файла по указанному пути
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var doc = ActiveMdiChild as FormNewDocument; // Активное окно
            if (doc != null) // Если док существует, приступаем к сохранению
            {
                var dlg = new SaveFileDialog(); // Выводим диалоговое окно с сохранением
                dlg.AddExtension = true;
                dlg.Filter = "Windows Bitmap (*.bmp)|*.bmp|Файлы JPEG (*.jpg)|*.jpg";
                ImageFormat[] ff = { ImageFormat.Bmp, ImageFormat.Jpeg };
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    doc.SaveAs(dlg.FileName, ff[dlg.FilterIndex - 1]); // Сохраняем док по пути с указанным форматом
                    ActiveMdiChild.Text = dlg.FileName; // Обновляем заголовок формы
                }

            }
        }

        /// <summary>
        /// Открытие документа по указанному пути
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartPosition = FormStartPosition.CenterParent; // Центрирование формы
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Windows Bitmap (*.bmp)|*.bmp|Файлы JPEG (*.jpeg, *.jpg)|*.jpeg;*.jpg";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var doc = new FormNewDocument(dlg.FileName);                          
                doc.MdiParent = this; // Устаналиваем родительскую форму
                doc.Show();
                ShowSize(doc.ClientSize.Width, doc.ClientSize.Height);
                ShowScale(1);
                LockToolbar(false);
            }
        }
        
        /// <summary>
        /// Выход из программы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// Изменение размеров холста
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void размерХолстаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var doc = ActiveMdiChild as FormNewDocument;
            // Получаем текущие размеры холста
            int currentWidth = doc.ClientSize.Width;
            int currentHeight = doc.ClientSize.Height;
            // Создаем и показываем диалоговое окно
            using (var resizeDialog = new FormCanvasSize(currentWidth, currentHeight))
            {
                if (resizeDialog.ShowDialog(this) == DialogResult.OK)
                {
                    doc.ResizeCanvas(resizeDialog.CanvasWidth, resizeDialog.CanvasHeight);
                    ShowSize(resizeDialog.CanvasWidth, resizeDialog.CanvasHeight); // Обновляем строку состояния
                }
            }
        }

        /// <summary>
        /// Демонстрация окна "О программе"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var aboutForm = new FormAboutProgram();
            aboutForm.ShowDialog();
        }

        #region Расположение окон

        /// <summary>
        /// Располагает открытые окна каскадно
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void каскадомToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        /// <summary>
        /// Располагает открытые окна вертикально (слева направо)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void слеваНаправоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        /// <summary>
        /// Располагает открытые окна горизонтально (сверху вниз)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void сверхуВToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        /// <summary>
        /// Упорядочивает открытые окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void упорядочитьЗначкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        #endregion

        #region Инструменты

        /// <summary>
        /// Смена инструмента на кисть
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            CurrentTool = Tools.Pen;
            Cursor = new Cursor("D:\\Microsoft Visual Studio\\2022\\repos\\Lab-1\\Lab-1\\Resources\\brush.cur");
        }

        /// <summary>
        /// Смена инструмента на окружность
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            CurrentTool = Tools.Circle;
            Cursor = new Cursor("D:\\Microsoft Visual Studio\\2022\\repos\\Lab-1\\Lab-1\\Resources\\brush.cur");
        }

        /// <summary>
        /// Смена инструмента на прямоугольник
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            CurrentTool = Tools.Rectangle;
            Cursor = new Cursor("D:\\Microsoft Visual Studio\\2022\\repos\\Lab-1\\Lab-1\\Resources\\brush.cur");
        }

        /// <summary>
        /// Смена инструмента на ластик
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            CurrentTool = Tools.Eraser;
            Cursor = new Cursor("D:\\Microsoft Visual Studio\\2022\\repos\\Lab-1\\Lab-1\\Resources\\eraser.cur");
        }

        /// <summary>
        /// Смена инструмента на текст
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            CurrentTool = Tools.Text;
            Cursor = new Cursor("D:\\Microsoft Visual Studio\\2022\\repos\\Lab-1\\Lab-1\\Resources\\text.cur");
        }

        /// <summary>
        /// Смена инструмента на ведро с краской
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            CurrentTool = Tools.BucketFill;
            Cursor = new Cursor("D:\\Microsoft Visual Studio\\2022\\repos\\Lab-1\\Lab-1\\Resources\\bucket.cur");
        }

        /// <summary>
        /// Смена инструмента на правильный n-угольник
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            CurrentTool = Tools.RegularPolygon;
            Cursor = new Cursor("D:\\Microsoft Visual Studio\\2022\\repos\\Lab-1\\Lab-1\\Resources\\polygon.cur");
        }

        /// <summary>
        /// Смена инструмента на линию
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            CurrentTool = Tools.Line;
            Cursor = new Cursor("D:\\Microsoft Visual Studio\\2022\\repos\\Lab-1\\Lab-1\\Resources\\line.cur");
        }

        #endregion

        #region Настройка кисти

        /// <summary>
        /// Выбор цвета кисти
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            var dlg = new ColorDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK)
                CurrentColor = dlg.Color;
        }

        /// <summary>
        /// Устанавливает толщину кисти размерностью 1 пкс
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void пксToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentWidth = 1;
        }

        /// <summary>
        /// Устанавливает толщину кисти размерностью 3 пкс
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void пксToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CurrentWidth = 3;
        }

        /// <summary>
        /// Устанавливает толщину кисти размерностью 5 пкс
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void пксToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            CurrentWidth = 5;
        }

        /// <summary>
        /// Устанавливает толщину кисти размерностью 8 пкс
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void пксToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            CurrentWidth = 8;
        }

        #endregion

        /// <summary>
        /// Переключение режима заливки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            IsFilled = !IsFilled; // Переключаем режим
            toolStripButton4.Text = IsFilled ? "Заливка: Вкл" : "Заливка: Выкл";
        }

        /// <summary>
        /// Блокировка или разблокировка кнопок на панели инструментов
        /// </summary>
        /// <param name="lockToolbar">
        /// Если значение параметра равно <c>true</c>, то элементы панели инструментов будут заблокированы, 
        /// если <c>false</c> - разблокированы.
        /// </param>
        private void LockToolbar(bool lockToolbar)
        {
            toolStripButton3.Enabled = !lockToolbar; // Цвет
            toolStripSplitButton1.Enabled = !lockToolbar; // Толщина
            toolStripButton6.Enabled = !lockToolbar; // Кисть
            toolStripButton1.Enabled = !lockToolbar; // Окружность
            toolStripButton5.Enabled = !lockToolbar; // Прямоугольник
            toolStripButton10.Enabled = !lockToolbar; // Линия
            toolStripButton9.Enabled = !lockToolbar; // Многоугольник
            toolStripButton7.Enabled = !lockToolbar; // Текст
            toolStripButton4.Enabled = !lockToolbar; // Заливка вкл/выкл
            toolStripButton2.Enabled = !lockToolbar; // Ластик
            toolStripButton8.Enabled = !lockToolbar; // Заливка (ведро)
            button1.Enabled = !lockToolbar; // Отменить изменения

            рисунокToolStripMenuItem.Enabled = !lockToolbar;
            окноToolStripMenuItem.Enabled = !lockToolbar;
        }

        #endregion

        #region Строка состояния

        /// <summary>
        /// Демонстрация текущей позиции курсора на окне
        /// </summary>
        /// <param name="x">Координата по оси X</param>
        /// <param name="y">Координата по оси Y</param>
        public void ShowPosition(int x, int y)
        {
            if (x != -1) // Если переданы корректные координаты
                statusLabelPosition.Text = $"X: {x}, Y: {y}";
            else
                statusLabelPosition.Text = string.Empty;
        }

        /// <summary>
        /// Демонстрация текущих размеров окна
        /// </summary>
        /// <param name="width">Ширина текущего окна</param>
        /// <param name="height">Высота текущего окна</param>
        public void ShowSize(int width, int height)
        {
            if (width == 0 && height == 0)
                statusLabelSize.Text = string.Empty;
            else
                statusLabelSize.Text = $"{width}×{height}";
        }

        /// <summary>
        /// Демонстрация текущего масштаба окна
        /// </summary>
        /// <param name="scale"></param>
        public void ShowScale(float scale)
        {
            if (scale < 0)
                toolStripStatusLabel1.Text = $"Масштаб: --";
            else
                toolStripStatusLabel1.Text = $"Масштаб: {scale * 100:0}%"; // Отображаем масштаб в процентах
        }

        /// <summary>
        /// Определение наличия активных окон
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_MdiChildActivate(object sender, EventArgs e)
        {
            var doc = ActiveMdiChild as FormNewDocument;
            if (doc != null)
            {
                LockToolbar(false);
                ShowSize(doc.ClientSize.Width, doc.ClientSize.Height);
            }
            else
            {
                LockToolbar(true);
                ShowSize(0, 0);
                ShowScale(-1);
            }
        }

        #endregion

        #region Работа с плагинами

        /// <summary>
        /// Поиск плагинов в папке проекта
        /// </summary>
        void FindPlugins()
        {
            ConfigClass config = LoadConfig(); // Загружаем конфигурацию из файла 
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            string[] files = Directory.GetFiles(folder, "*.dll"); // Ищем dll файлы в указанной папке
            foreach (var pluginEntry in config.Plugins)
            {
                if (pluginEntry.Enabled) // Загружаем только включенные плагины
                {
                    string filePath = Path.Combine(folder, pluginEntry.FileName); // Формируем путь к плагину
                    if (File.Exists(filePath))
                        LoadPluginFromFile(filePath); // Загружаем файл по пути
                    else
                        MessageBox.Show($"Файл не найден: {filePath}");
                }
            }
            SaveConfig(config); // Сохраняем конфиг
        }

        /// <summary>
        /// Загрузка плагина
        /// </summary>
        /// <param name="file">Путь к плагину</param>
        private void LoadPluginFromFile(string file)
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(file); // Загружаем сборку для проверки на наличие плагина
                foreach (Type type in assembly.GetTypes()) 
                {
                    // Проходимся по всем типам и ищем те, которые реализуют IPlugin
                    Type iface = type.GetInterface("PluginInterface.IPlugin");
                    if (iface != null)
                    {
                        IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                        plugins.Add(plugin.Name, plugin);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки плагина {Path.GetFileName(file)}: {ex.Message}");
            }
        }

        /// <summary>
        /// Загрузка конфигурации из файла
        /// </summary>
        /// <returns>Конфигурационный файл</returns>
        private ConfigClass LoadConfig()
        {
            if (!File.Exists(ConfigFilePath))
            {
                ConfigClass config = new ConfigClass();
                string folder = AppDomain.CurrentDomain.BaseDirectory;
                string[] files = Directory.GetFiles(folder, "*.dll", SearchOption.TopDirectoryOnly);

                foreach (string file in files)
                {
                    config.Plugins.Add(new PluginEntry
                    {
                        FileName = Path.GetFileName(file),
                        Enabled = true
                    });
                }
                SaveConfig(config);
                return config;
            }

            try // Если файл найден, пытаемся его прочитать
            {
                ConfigClass config = new ConfigClass();
                string[] lines = File.ReadAllLines(ConfigFilePath);
                foreach (string line in lines)
                {
                    string trimmed = line.Trim(); // Удаляем пробелы
                    if (string.IsNullOrEmpty(trimmed)) // Пустые строки пропускаем
                        continue;
                    if (trimmed == "[Plugins]") // Пропускаем заголовок
                        continue;
                    var parts = trimmed.Split('='); // Разделяем строку по символу
                    if (parts.Length == 2) // Если строка корректна, добавляем плагин
                    {
                        config.Plugins.Add(new PluginEntry
                        {
                            FileName = parts[0],
                            Enabled = bool.Parse(parts[1])
                        });
                    }
                }
                return config;
            }
            catch // Если при чтении возникла ошибка, резервный сценарий - создаем нвоый файлик
            {
                ConfigClass config = new ConfigClass();
                string folder = AppDomain.CurrentDomain.BaseDirectory;
                string[] files = Directory.GetFiles(folder, "*.dll", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    config.Plugins.Add(new PluginEntry
                    {
                        FileName = Path.GetFileName(file),
                        Enabled = true
                    });
                }
                SaveConfig(config);
                return config;
            }   
        }

        /// <summary>
        /// Сохранение конфигурации
        /// </summary>
        /// <param name="config">Конфигурационный файл</param>
        private void SaveConfig(ConfigClass config)
        {
            try
            {
                List<string> lines = new List<string> { "[Plugins]" };
                foreach (var plugin in config.Plugins)
                {
                    lines.Add($"{plugin.FileName}={plugin.Enabled}");
                }
                File.WriteAllLines(ConfigFilePath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка записи конфигурационного файла: {ex.Message}");
            }
        }

        /// <summary>
        /// Создание меню плагинов
        /// </summary>
        private void CreatePluginsMenu()
        {
            // Перебираем словарь с плагинами
            foreach (var plugin in plugins)
            {
                // Для каждого плагина новый пункт в меню
                var item = фильтрыToolStripMenuItem.DropDownItems.Add(plugin.Value.Name);
                // Если мы кликаем на плагин
                item.Click += async (sender, args) =>
                {
                    // Подгружаем документ и изображение
                    FormNewDocument activeChild = ActiveMdiChild as FormNewDocument;
                    if (activeChild != null && activeChild.bitmap != null)
                    {
                        try
                        {
                            cts = new CancellationTokenSource();
                            progressBar.Value = 0;
                            progressBar.Visible = true;
                            LockToolbar(true);
                            // Сохраняем копию изображения перед применением фильтра
                            activeChild.undoStack.Push(new Bitmap(activeChild.bitmap));
                            var progress = new Progress<int>(percent =>
                            {
                                if (this.InvokeRequired) // Код работает в фоновом потоке, непрямую к progressBar обратиться нельзя
                                    this.Invoke(new Action(() => progressBar.Value = percent)); // Блокируем фоновый поток, чтобы внесии изменения
                                else
                                    progressBar.Value = percent;
                            });

                            // Проверяем, что метод есть в плагине
                            var transformAsyncMethod = plugin.Value.GetType().GetMethod("TransformAsync");
                            if (transformAsyncMethod != null)
                            {
                                // Вызываем этот метод
                                await (Task)transformAsyncMethod.Invoke(plugin.Value, new object[] { activeChild.bitmap, progress, cts.Token });
                            }
                            else
                            {
                                var transformMethod = plugin.Value.GetType().GetMethod("Transform");
                                if (transformMethod != null)
                                {
                                    transformMethod.Invoke(plugin.Value, new object[] { activeChild.bitmap });
                                    progressBar.Value = 100;
                                }
                                else
                                {
                                    MessageBox.Show($"Плагин {plugin.Value.Name} не содержит метод Transform или TransformAsync.");
                                    return;
                                }
                            }

                            activeChild.IsModified = true;
                            activeChild.Invalidate();
                        }
                        catch (Exception ex)
                        {
                            // Восстанавливаем изображение при ошибке
                            if (activeChild.undoStack.Count > 0)
                            {
                                activeChild.bitmap.Dispose();
                                activeChild.bitmap = activeChild.undoStack.Pop();
                                activeChild.Invalidate();
                            }
                            MessageBox.Show($"Ошибка при применении фильтра: {ex.Message}");
                        }
                        finally
                        {
                            progressBar.Visible = false;
                            LockToolbar(false);
                            cts?.Dispose();
                            cts = null;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Нет активного документа или изображение не загружено.");
                    }
                };
            }
        }

        /// <summary>
        /// Демонстрация окна с плагинами
        /// </summary>
        private void ShowPluginDialog()
        {
            ConfigClass config = LoadConfig();
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            using (var dialog = new PluginForm(config, folder))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    SaveConfig(config);
                    plugins.Clear();
                    фильтрыToolStripMenuItem.DropDownItems.Clear();
                    // Добавляем пункт "Управление плагинами"
                    var managePluginsItem = фильтрыToolStripMenuItem.DropDownItems.Add("Управление плагинами");
                    managePluginsItem.Click += (s, e) => ShowPluginDialog();
                    фильтрыToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
                    FindPlugins();
                    CreatePluginsMenu();
                }
            }
        }
        
        /// <summary>
        /// Нажатие на список меню управления плагинами
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void управлениеПлагинамиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowPluginDialog();
        }
        
        /// <summary>
        /// Нажатие на кнопку отмены
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            var doc = ActiveMdiChild as FormNewDocument;
            if (doc != null && doc.undoStack.Count > 0)
            {
                // Освобождаем текущие изображения
                doc.bitmap?.Dispose();
                doc.bitmapTemp?.Dispose();
                doc.bitmap = doc.undoStack.Pop(); // Восстанавливаем bitmap из undoStack
                doc.bitmapTemp = new Bitmap(doc.bitmap); // Создаем новый bitmapTemp, клонируя bitmap
                doc.IsModified = true;
                doc.Invalidate();
            }
            else
            {
                MessageBox.Show("Нет действий для отмены.");
            }
        }

        #endregion
    }
}