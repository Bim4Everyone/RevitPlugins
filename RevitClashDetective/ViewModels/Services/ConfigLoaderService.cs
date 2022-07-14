using System;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.ViewModels.Services {
    internal class ConfigLoaderService {
        public T Load<T>() where T : ProjectConfig, new() {
            var openWindow = GetPlatformService<IOpenFileDialogService>();
            openWindow.Filter = "ClashConfig |*.json";
            if(!openWindow.ShowDialog(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))) {
                throw new OperationCanceledException();
            }

            try {
                var configLoader = new ConfigLoader();
                return configLoader.Load<T>(openWindow.File.FullName);
            } catch(pyRevitLabs.Json.JsonSerializationException) {
                ShowError();
                throw new OperationCanceledException();
            }
        }

        public void CheckConfig(FiltersConfig filtersConfig) {
            if(filtersConfig.Filters.Count == 0 && string.IsNullOrEmpty(filtersConfig.RevitVersion)) {
                ShowError();
                throw new OperationCanceledException();
            }
        }

        public void CheckConfig(ChecksConfig checksConfig) {
            if(checksConfig.Checks.Count == 0 && string.IsNullOrEmpty(checksConfig.RevitVersion)) {
                ShowError();
                throw new OperationCanceledException();
            }
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }

        private void ShowError() {
            var mb = GetPlatformService<IMessageBoxService>();
            mb.Show("Неверный файл конфигурации.", "BIM", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, System.Windows.MessageBoxResult.OK);
        }
    }
}
