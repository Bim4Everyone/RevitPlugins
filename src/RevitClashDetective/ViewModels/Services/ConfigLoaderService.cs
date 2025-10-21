using System;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.SimpleServices;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.ViewModels.Services;
internal class ConfigLoaderService {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;

    public ConfigLoaderService(RevitRepository revitRepository,
        ILocalizationService localization,
        IOpenFileDialogService openFileDialogService,
        IMessageBoxService messageBoxService) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
    }

    public IOpenFileDialogService OpenFileDialogService { get; }
    public IMessageBoxService MessageBoxService { get; }


    public T Load<T>() where T : ProjectConfig, new() {
        OpenFileDialogService.Filter = "ClashConfig |*.json";


        if(!OpenFileDialogService.ShowDialog(_revitRepository.GetFileDialogPath())) {
            throw new OperationCanceledException();
        }

        _revitRepository.CommonConfig.LastRunPath = OpenFileDialogService.File.DirectoryName;
        _revitRepository.CommonConfig.SaveProjectConfig();

        try {
            var configLoader = new ConfigLoader(_revitRepository.Doc);
            return configLoader.Load<T>(OpenFileDialogService.File.FullName);
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

    private void ShowError() {
        MessageBoxService.Show(_localization.GetLocalizedString("ConfigLoader.InvalidFile"),
            _localization.GetLocalizedString("BIM"),
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Error,
            System.Windows.MessageBoxResult.OK);
    }
}
