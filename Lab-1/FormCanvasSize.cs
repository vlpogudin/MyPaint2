using System;
using System.Windows.Forms;

namespace Lab_1
{
    /// <summary>
    /// Форма изменения размеров холста
    /// </summary>
    public partial class FormCanvasSize : Form
    {
        #region Свойства холста

        /// <summary>
        /// Ширина холста
        /// </summary>
        public int CanvasWidth { get; private set; }

        /// <summary>
        /// Высота холста
        /// </summary>
        public int CanvasHeight { get; private set; }

        #endregion

        #region Конструкторы холста

        /// <summary>
        /// Инициализация холста
        /// </summary>
        /// <param name="currentWidth">Текущая ширина</param>
        /// <param name="currentHeight">Текущая высота</param>
        public FormCanvasSize(int currentWidth, int currentHeight)
        {
            InitializeComponent();
            textWidth.Text = currentWidth.ToString();
            textHeight.Text = currentHeight.ToString();
        }

        #endregion

        #region Нажатие на клавиши

        /// <summary>
        /// Обработка нажатия на клавишу ОК
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click_1(object sender, EventArgs e)
        {
            while (true)
            {
                if (int.TryParse(textWidth.Text, out int width) && int.TryParse(textHeight.Text, out int height))
                {
                    if (width > 119 && height > 0)
                    {
                        CanvasWidth = width;
                        CanvasHeight = height;
                        this.DialogResult = DialogResult.OK; // Установите результат диалога
                        this.Close(); // Закрытие формы, если ввод валиден
                        return; // Выход из метода
                    }
                    else
                    {
                        MessageBox.Show("Ширина должна быть не менее 120, высота больше нуля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Введены некорректные данные. Введите целочисленные значения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                this.DialogResult = DialogResult.None;
                textWidth.Focus(); // Установка фокуса на поле ввода
                textWidth.SelectAll(); // Выделение текста для повторного ввода
                break;
            }
        }

        #endregion
    }
}