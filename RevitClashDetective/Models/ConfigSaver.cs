using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models {
    internal class ConfigSaver {
        private IConfigSerializer _serializer = new RevitClashConfigSerializer();
        public bool Save(ProjectConfig config) {
            var saveWindow = GetPlatformService<ISaveFileDialogService>();
            saveWindow.AddExtension = true;
            saveWindow.Filter = "ClashConfig |*.json";
            saveWindow.FilterIndex = 1;
            if(saveWindow.ShowDialog(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "config")) {
                File.WriteAllText(Path.Combine(saveWindow.File.FullName), _serializer.Serialize(config));
                return true;
            }
            return false;
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }
    }
}
