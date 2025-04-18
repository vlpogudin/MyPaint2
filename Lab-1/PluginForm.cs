using PluginInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Lab_1
{
    public partial class PluginForm : Form
    {
        /// <summary>
        /// Класс информации плагина
        /// </summary>
        private class PluginInfo
        {
            /// <summary>
            /// Наименование плагина
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Файл плагина
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// Вкл/выкл. плагина
            /// </summary>
            public bool Enabled { get; set; }
        }

        /// <summary>
        /// Информация о плагине
        /// </summary>
        private readonly List<PluginInfo> pluginInfos; 

        /// <summary>
        /// Конфигурационный файл
        /// </summary>
        private readonly ConfigClass config;

        /// <summary>
        /// Папка с плагинами
        /// </summary>
        private readonly string pluginsFolder;

        /// <summary>
        /// Конструктор для инициализации формы
        /// </summary>
        /// <param name="config">Конфигурационный файл</param>
        /// <param name="pluginsFolder">Папка с плагинами</param>
        public PluginForm(ConfigClass config, string pluginsFolder)
        {
            InitializeComponent();
            this.config = config;
            this.pluginsFolder = pluginsFolder;
            pluginInfos = new List<PluginInfo>();

            // Настройка DataGridView
            pluginsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            pluginsGrid.AllowUserToAddRows = false;
            pluginsGrid.AllowUserToDeleteRows = false;
            pluginsGrid.Columns.Clear();
            pluginsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Название",
                ReadOnly = true // Только для чтения
            });
            pluginsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Author",
                HeaderText = "Автор",
                ReadOnly = true // Только для чтения
            });
            pluginsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Version",
                HeaderText = "Версия",
                ReadOnly = true // Только для чтения
            });
            pluginsGrid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Enabled",
                HeaderText = "Включен",
                ValueType = typeof(bool),
                ReadOnly = false // Редактируемый
            });

            // Заполнение таблицы данными о плагинах
            LoadPlugins();
        }

        /// <summary>
        /// Загрузка плагинов в таблицу
        /// </summary>
        private void LoadPlugins()
        {
            string[] files = Directory.GetFiles(pluginsFolder, "*.dll");
            foreach (string file in files)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(file);
                    foreach (Type type in assembly.GetTypes())
                    {
                        Type iface = type.GetInterface("PluginInterface.IPlugin");
                        if (iface != null)
                        {
                            IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                            var versionAttr = (VersionAttribute)Attribute.GetCustomAttribute(type, typeof(VersionAttribute));
                            string version = versionAttr != null ? $"{versionAttr.Major}.{versionAttr.Minor}" : "Не указана";

                            var entry = config.Plugins.Find(p => p.FileName.Equals(Path.GetFileName(file), StringComparison.OrdinalIgnoreCase));
                            bool enabled = entry?.Enabled ?? true;

                            pluginsGrid.Rows.Add(plugin.Name, plugin.Author, version, enabled);
                            pluginInfos.Add(new PluginInfo
                            {
                                Name = plugin.Name,
                                FileName = Path.GetFileName(file),
                                Enabled = enabled
                            });
                        }
                    }
                }
                catch
                {
                    // Игнорируем DLL, которые не являются плагинами
                }
            }
        }

        /// <summary>
        /// Обновление конфигурации
        /// </summary>
        private void UpdateConfig()
        {
            config.Plugins.Clear();
            foreach (DataGridViewRow row in pluginsGrid.Rows)
            {
                string fileName = pluginInfos[row.Index].FileName;
                bool enabled = (bool)row.Cells["Enabled"].Value;
                config.Plugins.Add(new PluginEntry { FileName = fileName, Enabled = enabled });
            }
        }

        private void saveButton_Click_1(object sender, EventArgs e)
        {
            UpdateConfig();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click_1(object sender, EventArgs e)
        {
            Close();
        }
    }
}
