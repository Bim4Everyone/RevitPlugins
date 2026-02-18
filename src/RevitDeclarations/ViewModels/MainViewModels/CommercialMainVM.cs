using System;
using System.Linq;

using Autodesk.Revit.UI;

using RevitDeclarations.Models;
using RevitDeclarations.Views;

using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using TaskDialogResult = Autodesk.Revit.UI.TaskDialogResult;

namespace RevitDeclarations.ViewModels;
internal class CommercialMainVM : MainViewModel {
    private new readonly CommercialSettings _settings;

    public CommercialMainVM(RevitRepository revitRepository, CommercialSettings settings)
        : base(revitRepository, settings) {
        _settings = settings;

        _declarationViewModel = new DeclarationCommercialVM(_revitRepository, settings);
        _parametersViewModel = new CommercialParamsVM(_revitRepository, this);

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
            .Where(x => x.Errors.Any());
        if(parameterErrors.Any()) {
            var window = new ErrorWindow() { DataContext = new ErrorsViewModel(parameterErrors, false) };
            window.ShowDialog();
            return;
        }

        var projects = checkedDocuments
            .Select(x => new CommercialProject(x, _revitRepository, _settings))
            .ToList();

        // Проверка 2. Наличие групп помещений на выбранной стадии во всех выбранных проектах.
        var noApartsErrors = projects
            .Select(x => x.CheckRoomGroupsInProject())
            .Where(x => x.Errors.Any());
        if(noApartsErrors.Any()) {
            var window = new ErrorWindow() { DataContext = new ErrorsViewModel(noApartsErrors, false) };
            window.ShowDialog();
            return;
        }

        // Проверка 3. У каждого помещения должны быть актуальные площади помещения из кватирографии.
        var actualRoomAreasErrors = projects
            .Select(x => x.CheckActualRoomAreas())
            .Where(x => x.Errors.Any());
        if(actualRoomAreasErrors.Any()) {
            var window = new ErrorWindow() {
                DataContext = new ErrorsViewModel(actualRoomAreasErrors, true)
            };
            window.ShowDialog();

            if(!(bool) window.DialogResult) {
                return;
            }
        }

        var commercialRooms = projects
            .SelectMany(x => x.RoomGroups)
            .Cast<CommercialRooms>()
            .OrderBy(x => x.Number ?? "", _stringComparer)
            .ThenBy(x => x.Rooms.First().Number ?? "", _stringComparer)
            .ToList();

        _declarationViewModel.SelectedFormat.Export(_declarationViewModel.FullPath, commercialRooms);
        try {
        } catch(Exception e) {
            var taskDialog = new TaskDialog("Ошибка выгрузки") {
                CommonButtons = TaskDialogCommonButtons.No | TaskDialogCommonButtons.Yes,
                MainContent = "Произошла ошибка выгрузки.\nПопробовать выгрузить декларацию в формате csv?",
                ExpandedContent = $"Описание ошибки: {e.Message}"
            };

            var dialogResult = taskDialog.Show();

            if(dialogResult == TaskDialogResult.Yes) {
                _declarationViewModel.SelectedFormat.Export(_declarationViewModel.FullPath, commercialRooms);
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
