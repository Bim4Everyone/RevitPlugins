using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone.ProjectConfigs;

using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models {
    internal class ConfigSaver {
        private IConfigSerializer _serializer = new RevitClashConfigSerializer();
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
