using System;
using System.Linq;

using Autodesk.Revit.UI;

using RevitDeclarations.Models;
using RevitDeclarations.Services;
using RevitDeclarations.ViewModels;
using RevitDeclarations.Views;

using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using TaskDialogResult = Autodesk.Revit.UI.TaskDialogResult;

namespace RevitDeclarations.ViewModels;
internal class PublicAreasMainVM : MainViewModel {
    private new readonly PublicAreasSettings _settings;

    public PublicAreasMainVM(RevitRepository revitRepository, 
                             PublicAreasSettings settings, 
                             ErrorWindowService errorWindowService)
        : base(revitRepository, settings, errorWindowService) {
        _settings = settings;

        _declarationViewModel = new DeclarationPublicAreasVM(_revitRepository, settings);
        _parametersViewModel = new PublicAreasParamsVM(_revitRepository, this);
        _prioritiesViewModel = new PrioritiesViewModel(this);

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
