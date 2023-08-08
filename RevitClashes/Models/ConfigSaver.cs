using System;
using System.IO;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectConfigs;

using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models {
    internal class ConfigSaver {
        private readonly IConfigSerializer _serializer;


        public ConfigSaver(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            _serializer = new RevitClashConfigSerializer(new RevitClashesSerializationBinder(), document);
        }


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
