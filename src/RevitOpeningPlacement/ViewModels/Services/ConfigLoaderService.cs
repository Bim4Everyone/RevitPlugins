using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.SimpleServices;

using RevitClashDetective.Models;

namespace RevitOpeningPlacement.ViewModels.Services;
internal class ConfigLoaderService {
    private readonly IOpenFileDialogService _openFileDialogService;
    private readonly IMessageBoxService _messageBoxService;

    public ConfigLoaderService(IOpenFileDialogService openFileDialogService, IMessageBoxService messageBoxService) {
        _openFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
    }

    public T Load<T>(Document document) where T : ProjectConfig, new() {
        if(document is null) { throw new ArgumentNullException(nameof(document)); }

        _openFileDialogService.Filter = "OpeningConfig |*.json";
        if(!_openFileDialogService.ShowDialog(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))) {
            throw new OperationCanceledException();
        }

        try {
            var configLoader = new ConfigLoader(document);
            var config = configLoader.Load<T>(_openFileDialogService.File.FullName);
            config.ProjectConfigPath = _openFileDialogService.File.FullName;
            return config;
        } catch(pyRevitLabs.Json.JsonSerializationException) {
            ShowError();
            throw new OperationCanceledException();
        }
    }

    public void CheckConfig(Models.Configs.OpeningConfig openingConfig) {
        if(openingConfig.Categories.Count == 0 && string.IsNullOrEmpty(openingConfig.RevitVersion)) {
            ShowError();
            throw new OperationCanceledException();
        }
    }

    private void ShowError() {
        _messageBoxService.Show("Неверный файл конфигурации.",
            "BIM",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Error,
            System.Windows.MessageBoxResult.OK);
    }
}
