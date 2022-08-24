using System;
using System.IO;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

namespace RevitLintelPlacement.Models {
    internal class ConfigSaver {
        private IConfigSerializer _serializer = new ConfigSerializer();
        public void Save(ProjectConfig config, string configPath) {
            if(config is null) {
                throw new ArgumentNullException(nameof(config));
            }

            if(string.IsNullOrEmpty(configPath)) {
                throw new ArgumentException($"'{nameof(configPath)}' cannot be null or empty.", nameof(configPath));
            }

            File.WriteAllText(configPath, _serializer.Serialize(config));
        }
    }
}
