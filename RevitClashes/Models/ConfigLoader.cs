using System;
using System.IO;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectConfigs;

using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models {
    internal class ConfigLoader {
        private readonly IConfigSerializer _serializer;
        private readonly Document _document;


        public ConfigLoader(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            _document = document;
            _serializer = new RevitClashConfigSerializer(new RevitClashesSerializationBinder(), _document);
        }


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
