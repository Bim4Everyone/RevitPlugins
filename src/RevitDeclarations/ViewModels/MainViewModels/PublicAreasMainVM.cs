using System;
using System.Linq;
using System.Windows;

using Autodesk.Revit.UI;

using dosymep.SimpleServices;

using RevitDeclarations.Models;
using RevitDeclarations.Services;
using RevitDeclarations.ViewModels;
using RevitDeclarations.Views;


namespace RevitDeclarations.ViewModels;
internal class PublicAreasMainVM : MainViewModel {
    private new readonly PublicAreasSettings _settings;

    public PublicAreasMainVM(RevitRepository revitRepository, 
                             PublicAreasSettings settings,
                             IMessageBoxService messageBoxService,
                             ErrorWindowService errorWindowService)
        : base(revitRepository, settings, messageBoxService, errorWindowService) {
        _settings = settings;

        _declarationViewModel = new DeclarationPublicAreasVM(_revitRepository, settings, messageBoxService);
        _parametersViewModel = new PublicAreasParamsVM(_revitRepository, this);
        _prioritiesViewModel = new PrioritiesViewModel(this, messageBoxService);

        LoadConfig();
    }

    public override void ExportDeclaration(object obj) {
        SetSelectedSettings();
        SetPublicAreasSettings();

        var checkedDocuments = _declarationViewModel.RevitDocuments
            .Where(x => x.IsChecked)
            .ToList();

        SaveConfig();

        // Проверка 1. Наличие параметров во всех выбранных проектах.
        var parameterErrors = checkedDocuments
            .Select(x => x.CheckParameters())
            .Where(x => x.Elements.Any());
        if(parameterErrors.Any()) {
            _errorWindowService.ShowNoticeWindow(parameterErrors.ToList());
            return;
        }

        var projects = checkedDocuments
            .Select(x => new PublicAreasProject(x, _revitRepository, _settings))
            .ToList();

        // Проверка 2. Наличие групп помещений на выбранной стадии во всех выбранных проектах.
        var noApartsErrors = projects
            .Select(x => x.CheckRoomGroupsInProject())
            .Where(x => x.Elements.Any());
        if(noApartsErrors.Any()) {
            _errorWindowService.ShowNoticeWindow(noApartsErrors.ToList());
            return;
        }

        // Проверка 3. У каждого помещения должны быть актуальные площади помещения из кватирографии.
        var actualRoomAreasErrors = projects
            .Select(x => x.CheckActualRoomAreas())
            .Where(x => x.Elements.Any());
        if(actualRoomAreasErrors.Any()) {
            bool windowResult = _errorWindowService.ShowNoticeWindow(actualRoomAreasErrors.ToList(), true);

            if(!windowResult) {
                return;
            }
        }

        var commercialRooms = projects
            .SelectMany(x => x.RoomGroups)
            .Cast<PublicArea>()
            .OrderByDescending(x => x.IsUnderground)
            .ThenBy(x => x.RoomPosition ?? "", _stringComparer)
            .ThenBy(x => x.Rooms.First().Number ?? "", _stringComparer)
            .ToList();

        try {
            _declarationViewModel.SelectedFormat.Export(_declarationViewModel.FullPath, commercialRooms);
        } catch(Exception e) {
            var messageBoxResult = _messageBoxService.Show(
                $"Произошла ошибка выгрузки: {e.Message}.\n\nПопробовать выгрузить декларацию в формате csv?",
                "Ошибка выгрузки",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if(messageBoxResult == MessageBoxResult.Yes) {
                (_declarationViewModel as DeclarationPublicAreasVM)
                    .CsvExportViewModel.Export(_declarationViewModel.FullPath, commercialRooms);
            }
        }
    }

    private void SaveConfig() {
        var config = PublicAreasConfig.GetPluginConfig();

        var settings = _settings;

        var configSettings =
            config.GetSettings(_revitRepository.Document) ?? config.AddSettings(_revitRepository.Document);

        SaveMainWindowConfig(configSettings);
        SaveParametersConfig(configSettings);

        configSettings.AddPrefixToNumber = settings.AddPrefixToNumber;
        if(settings.AddPrefixToNumber) {
            configSettings.RoomNumberParam = settings.RoomNumberParam?.Definition.Name;
        }

        config.SaveProjectConfig();
    }

    private void LoadConfig() {
        var config = PublicAreasConfig.GetPluginConfig();
        var configSettings = config.GetSettings(_revitRepository.Document);

        if(configSettings is null) {
            return;
        }

        LoadMainWindowConfig(configSettings);
        _parametersViewModel.SetParametersFromConfig(configSettings);

        config.SaveProjectConfig();
    }

    private void SetPublicAreasSettings() {
        var publicAreasParamsVM = (PublicAreasParamsVM) _parametersViewModel;
        _settings.AddPrefixToNumber = publicAreasParamsVM.AddPrefixToNumber;
    }
}
