using System;
using System.IO;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models {
    internal class ConfigLoader {
        private IConfigSerializer _serializer = new RevitClashConfigSerializer();
        public T Load<T>() where T : ProjectConfig, new() {
            var openWindow = GetPlatformService<IOpenFileDialogService>();
            openWindow.Filter = "ClashConfig |*.json";
            if(openWindow.ShowDialog(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))) {
                string fileContent = File.ReadAllText(openWindow.File.FullName);

                try {
                    T projectConfig = _serializer.Deserialize<T>(fileContent);
                    projectConfig.Serializer = _serializer;

                    return projectConfig;
                } catch(pyRevitLabs.Json.JsonSerializationException) {
                    var mb = GetPlatformService<IMessageBoxService>();
                    mb.Show("Неверный файл конфигурации.", "BIM", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, System.Windows.MessageBoxResult.OK);
                    return null;
                }

            }
            return null;
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }
    }
}
