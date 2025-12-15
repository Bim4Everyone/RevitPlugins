using System;
using System.Linq;

using Autodesk.Revit.UI;

using RevitDeclarations.Models;
using RevitDeclarations.Views;

using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using TaskDialogResult = Autodesk.Revit.UI.TaskDialogResult;

namespace RevitDeclarations.ViewModels;
internal class PublicAreasMainVM : MainViewModel {
    private readonly PublicAreasExcelExportVM _excelExportViewModel;
    private readonly PublicAreasCsvExportVM _csvExportViewModel;

    private new readonly PublicAreasSettings _settings;

    public PublicAreasMainVM(RevitRepository revitRepository, PublicAreasSettings settings)
        : base(revitRepository, settings) {
        _settings = settings;

        _excelExportViewModel =
            new PublicAreasExcelExportVM("Excel", new Guid("186F3EEE-303A-42DF-910E-475AD2525ABD"), _settings);
        _csvExportViewModel =
            new PublicAreasCsvExportVM("csv", new Guid("A674AB16-642A-4642-BE51-51B812378734"), _settings);

        _exportFormats = [
            _excelExportViewModel,
            _csvExportViewModel
        ];
        _selectedFormat = _exportFormats[0];

        _parametersViewModel = new PublicAreasParamsVM(_revitRepository, this);
        _prioritiesViewModel = new PrioritiesViewModel(this);

        _loadUtp = false;
        _canLoadUtp = false;

        LoadConfig();
    }

    public override void ExportDeclaration(object obj) {
        SetSelectedSettings();
        SetPublicAreasSettings();

        var checkedDocuments = _revitDocuments
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
            .Select(x => new PublicAreasProject(x, _revitRepository, _settings))
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
            .Cast<PublicArea>()
            .OrderByDescending(x => x.IsUnderground)
            .ThenBy(x => x.RoomPosition ?? "", _stringComparer)
            .ThenBy(x => x.Rooms.First().Number ?? "", _stringComparer)
            .ToList();

        try {
            _selectedFormat.Export(FullPath, commercialRooms);
        } catch(Exception e) {
            var taskDialog = new TaskDialog("Ошибка выгрузки") {
                CommonButtons = TaskDialogCommonButtons.No | TaskDialogCommonButtons.Yes,
                MainContent = "Произошла ошибка выгрузки.\nПопробовать выгрузить декларацию в формате csv?",
                ExpandedContent = $"Описание ошибки: {e.Message}"
            };

            var dialogResult = taskDialog.Show();

            if(dialogResult == TaskDialogResult.Yes) {
                _csvExportViewModel.Export(FullPath, commercialRooms);
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
