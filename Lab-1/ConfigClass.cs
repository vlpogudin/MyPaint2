using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lab_1
{
    public class ConfigClass
    {
        public List<PluginEntry> Plugins { get; set; } = new List<PluginEntry>();
    }

    public class PluginEntry
    {
        public string FileName { get; set; }

        public bool Enabled { get; set; }
    }
}
