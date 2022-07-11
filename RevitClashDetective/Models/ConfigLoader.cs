using System;
using System.IO;

using dosymep.Bim4Everyone.ProjectConfigs;

using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models {
    internal class ConfigLoader {
        private IConfigSerializer _serializer = new RevitClashConfigSerializer();
        public T Load<T>(string configPath) where T : ProjectConfig, new() {
            if(string.IsNullOrEmpty(configPath)) {
                throw new ArgumentException($"'{nameof(configPath)}' cannot be null or empty.", nameof(configPath));
            }

            if(!File.Exists(configPath)) {
                throw new ArgumentException($"Путь \"{configPath}\" недоступен.", nameof(configPath));
            }

            string fileContent = File.ReadAllText(configPath);

            T projectConfig = _serializer.Deserialize<T>(fileContent);
            projectConfig.Serializer = _serializer;

            return projectConfig;
        }
    }
}
