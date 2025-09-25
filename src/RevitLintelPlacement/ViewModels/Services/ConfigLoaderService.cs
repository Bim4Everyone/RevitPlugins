using System;
using System.Windows;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using pyRevitLabs.Json;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels.Services;

internal class ConfigLoaderService {
    public T Load<T>() where T : ProjectConfig, new() {
        var openWindow = GetPlatformService<IOpenFileDialogService>();
        openWindow.Filter = "RuleConfig |*.json";
        if(!openWindow.ShowDialog(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))) {
            throw new OperationCanceledException();
        }

        try {
            var configLoader = new ConfigLoader();
            return configLoader.Load<T>(openWindow.File.FullName);
        } catch(JsonSerializationException) {
            ShowError();
            throw new OperationCanceledException();
        }
    }

    // public void CheckConfig(RuleConfig filtersConfig) {
    //    if(filtersConfig.RuleSettings.Count == 0 && string.IsNullOrEmpty(filtersConfig.RevitVersion)) {
    //        ShowError();
    //        throw new OperationCanceledException();
    //    }
    // }

    protected T GetPlatformService<T>() {
        return ServicesProvider.GetPlatformService<T>();
    }

    private void ShowError() {
        var mb = GetPlatformService<IMessageBoxService>();
        mb.Show(
            "Неверный файл конфигурации.",
            "BIM",
            MessageBoxButton.OK,
            MessageBoxImage.Error,
            MessageBoxResult.OK);
    }
}
