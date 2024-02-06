using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using RevitClashDetective.Models;

namespace RevitOpeningPlacement.ViewModels.Services {
    internal class ConfigSaverService {
        public void Save(ProjectConfig config, Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            var saveWindow = GetPlatformService<ISaveFileDialogService>();
            saveWindow.AddExtension = true;
            saveWindow.Filter = "ClashConfig |*.json";
            saveWindow.FilterIndex = 1;
            if(!saveWindow.ShowDialog(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "config")) {
                throw new OperationCanceledException();
            }
            var configSaver = new ConfigSaver(document);
            configSaver.Save(config, saveWindow.File.FullName);
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }
    }
}
