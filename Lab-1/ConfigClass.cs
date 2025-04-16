using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lab_1
{
    public class ConfigClass
    {
        [JsonPropertyName("AutoLoad")] public bool AutoLoad { get; set; } = true;
        [JsonPropertyName("Plugins")]
        public List<PluginEntry> Plugins { get; set; } = new List<PluginEntry>();
    }

    public class PluginEntry
    {
        [JsonPropertyName("FileName")]
        public string FileName { get; set; }

        [JsonPropertyName("Enabled")]
        public bool Enabled { get; set; }
    }
}
