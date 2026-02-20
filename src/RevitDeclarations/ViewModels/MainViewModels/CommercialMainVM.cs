using System;
using System.Linq;
using System.Windows;

using Autodesk.Revit.UI;

using dosymep.SimpleServices;

using RevitDeclarations.Models;
using RevitDeclarations.Services;
using RevitDeclarations.Views;

namespace RevitDeclarations.ViewModels;
internal class CommercialMainVM : MainViewModel {
    private new readonly CommercialSettings _settings;

    public CommercialMainVM(RevitRepository revitRepository, 
                            CommercialSettings settings,
                            ILocalizationService localizationService,
                            IMessageBoxService messageBoxService,
                            ErrorWindowService errorWindowService)
        : base(revitRepository, settings, localizationService, messageBoxService, errorWindowService) {
        _settings = settings;

        _declarationViewModel = new DeclarationCommercialVM(_revitRepository, settings, messageBoxService);
        _parametersViewModel = new CommercialParamsVM(_revitRepository, this);
        _prioritiesViewModel = new PrioritiesViewModel(this, messageBoxService);

        LoadConfig();
    }

    public override void ExportDeclaration(object obj) {
        SetSelectedSettings();
        SetCommSettings();

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
            .Select(x => new CommercialProject(x, _revitRepository, _settings))
            .ToList();

        // Проверка 2. Наличие групп помещений на выбранной стадии во всех выбранных проектах.
        var noGroupsErrors = projects
            .Select(x => x.CheckRoomGroupsInProject())
            .Where(x => x.Elements.Any());
        if(noGroupsErrors.Any()) {
            _errorWindowService.ShowNoticeWindow(noGroupsErrors.ToList());
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
            .Cast<CommercialRooms>()
            .OrderBy(x => x.Number ?? "", _stringComparer)
            .ThenBy(x => x.Rooms.First().Number ?? "", _stringComparer)
            .ToList();

        try {
            _declarationViewModel.SelectedFormat.Export(_declarationViewModel.FullPath, commercialRooms);
        } catch(Exception e) {
            var messageBoxResult = _messageBoxService.Show(
                _localizationService.GetLocalizedString("MessageBox.ExcelErrorLoadCsv", e.Message),
                _localizationService.GetLocalizedString("MessageBox.ExportErrorTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if(messageBoxResult == MessageBoxResult.Yes) {
                (_declarationViewModel as DeclarationCommercialVM)
                    .CsvExportViewModel.Export(_declarationViewModel.FullPath, commercialRooms);
            }
        }
    }

    private void SaveConfig() {
        var settings = _settings;

        var config = CommercialConfig.GetPluginConfig();

        var configSettings =
            config.GetSettings(_revitRepository.Document) ?? config.AddSettings(_revitRepository.Document);

        SaveMainWindowConfig(configSettings);
        SaveParametersConfig(configSettings);

        configSettings.BuildingNumberParam = settings.BuildingNumberParam?.Definition.Name;
        configSettings.ConstrWorksNumberParam = settings.ConstrWorksNumberParam?.Definition.Name;
        configSettings.RoomsHeightParam = settings.RoomsHeightParam?.Definition.Name;
        configSettings.GroupNameParam = settings.GroupNameParam?.Definition.Name;
        configSettings.ParkingSpaceClass = settings.ParkingSpaceClass?.Definition.Name;
        configSettings.ParkingInfo = settings.ParkingInfo?.Definition.Name;
        configSettings.AddPrefixToNumber = settings.AddPrefixToNumber;
        if(settings.AddPrefixToNumber) {
            configSettings.RoomNumberParam = settings.RoomNumberParam?.Definition.Name;
        }

        config.SaveProjectConfig();
    }

    private void LoadConfig() {
        var config = CommercialConfig.GetPluginConfig();
        var configSettings = config.GetSettings(_revitRepository.Document);

        if(configSettings is null) {
            return;
        }

        LoadMainWindowConfig(configSettings);
        _parametersViewModel.SetParametersFromConfig(configSettings);

        config.SaveProjectConfig();
    }

    private void SetCommSettings() {
        var commercialParamsVM = (CommercialParamsVM) _parametersViewModel;
        _settings.BuildingNumberParam = commercialParamsVM.SelectedBuildingNumberParam;
        _settings.ConstrWorksNumberParam = commercialParamsVM.SelectedConstrWorksNumberParam;
        _settings.RoomsHeightParam = commercialParamsVM.SelectedRoomsHeightParam;
        _settings.ParkingSpaceClass = commercialParamsVM.SelectedParkingSpaceClass;
        _settings.ParkingInfo = commercialParamsVM.SelectedParkingInfo;
        _settings.GroupNameParam = commercialParamsVM.SelectedGroupNameParam;
        _settings.AddPrefixToNumber = commercialParamsVM.AddPrefixToNumber;
    }
}
