using System;
using System.Windows.Forms;

namespace Lab_1
{
    /// <summary>
    /// Форма для ввода количества сторон правильного n-угольника
    /// </summary>
    public partial class FormPolygonSides : Form
    {
        #region Поля формы

        /// <summary>
        /// Количество сторон правильного n-угольника (по умолчанию 3)
        /// </summary>
        public int NumberOfSides { get; private set; } = 3;

        #endregion

        #region Конструкторы формы

        /// <summary>
        /// Инициализация формы
        /// </summary>
        public FormPolygonSides()
        {
            InitializeComponent();
        }

        #endregion

        #region Нажатие на клавиши

        /// <summary>
        /// Обработка нажатия на кнопку ОК
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click_1(object sender, EventArgs e)
        {
            if (int.TryParse(textSides.Text, out int sides) && sides >= 3)
            {
                NumberOfSides = sides;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Введите целое число сторон (минимум 3).", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработка нажатия на кнопку Отмена
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion
    }
}