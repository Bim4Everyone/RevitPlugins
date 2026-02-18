using System;
using System.Linq;

using Autodesk.Revit.UI;

using RevitDeclarations.Models;
using RevitDeclarations.ViewModels;
using RevitDeclarations.Views;

using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using TaskDialogResult = Autodesk.Revit.UI.TaskDialogResult;

namespace RevitDeclarations.ViewModels;
internal class ApartmentsMainVM : MainViewModel {
    private new readonly ApartmentsSettings _settings;

    public ApartmentsMainVM(RevitRepository revitRepository, ApartmentsSettings settings)
        : base(revitRepository, settings) {
        _settings = settings;

        _declarationViewModel = new DeclarationApartVM(_revitRepository, settings);
        _parametersViewModel = new ApartmentsParamsVM(_revitRepository, this);
        _prioritiesViewModel = new PrioritiesViewModel(this);

        LoadConfig();
    }

    public override void ExportDeclaration(object obj) {
        SetSelectedSettings();
        SetApartSettings();

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
            .Select(x => new ApartmentsProject(x, _revitRepository, _settings))
            .ToList();

        // Проверка 2. Наличие квартир на выбранной стадии во всех выбранных проектах.
        var noApartsErrors = projects
            .Select(x => x.CheckRoomGroupsInProject())
            .Where(x => x.Errors.Any());
        if(noApartsErrors.Any()) {
            var window = new ErrorWindow() { DataContext = new ErrorsViewModel(noApartsErrors, false) };
            window.ShowDialog();
            return;
        }

        // Проверка 3. У всех помещений каждой квартиры должны совпадать общие площади квартиры.
        var areasErrors = projects
            .Select(x => x.CheckRoomAreasEquality())
            .Where(x => x.Errors.Any());
        if(areasErrors.Any()) {
            var window = new ErrorWindow() { DataContext = new ErrorsViewModel(areasErrors, false) };
            window.ShowDialog();
            return;
        }

        // Проверка 4. У каждого помещения должны быть актуальные площади помещения из кватирографии.
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

        // Проверка 5. У каждого помещения должны быть актуальные площади квартиры из кватирографии.
        var actualApartmentAreasErrors = projects
            .Select(x => x.CheckActualApartmentAreas())
            .Where(x => x.Errors.Any());
        if(actualApartmentAreasErrors.Any()) {
            var window = new ErrorWindow() {
                DataContext = new ErrorsViewModel(actualApartmentAreasErrors, true)
            };
            window.ShowDialog();

            if(!(bool) window.DialogResult) {
                return;
            }
        }

        if(_declarationViewModel.LoadUtp) {
            // Проверка 6. Проверка проекта для корректной выгрузки УТП.
            var utpErrors = projects
                .Select(x => x.CheckUtpWarnings())
                .SelectMany(x => x)
                .Where(x => x.Errors.Any());
            if(utpErrors.Any()) {
                var window = new ErrorWindow() {
                    DataContext = new ErrorsViewModel(utpErrors, true)
                };
                window.ShowDialog();

                if(!(bool) window.DialogResult) {
                    return;
                }
            }

            foreach(var project in projects) {
                project.CalculateUtpForApartments();
            }
        }

        var apartments = projects
            .SelectMany(x => x.RoomGroups)
            .Cast<Apartment>()
            .OrderBy(x => x.Section ?? "", _stringComparer)
            .ThenBy(x => x.FullNumber ?? "", _stringComparer)
            .ToList();

        try {
            _declarationViewModel.SelectedFormat.Export(_declarationViewModel.FullPath, apartments);
        } catch(Exception e) {
            var taskDialog = new TaskDialog("Ошибка выгрузки") {
                CommonButtons = TaskDialogCommonButtons.No | TaskDialogCommonButtons.Yes,
                MainContent = "Произошла ошибка выгрузки.\nПопробовать выгрузить декларацию в формате csv?",
                ExpandedContent = $"Описание ошибки: {e.Message}"
            };

            var dialogResult = taskDialog.Show();

            if(dialogResult == TaskDialogResult.Yes) {
                (_declarationViewModel as DeclarationApartVM)
                    .CsvExportViewModel.Export(_declarationViewModel.FullPath, apartments);
            }
        }
    }

    private void SaveConfig() {
        var config = ApartmentsConfig.GetPluginConfig();

        var settings = _settings;

        var configSettings =
            config.GetSettings(_revitRepository.Document) ?? config.AddSettings(_revitRepository.Document);

        SaveMainWindowConfig(configSettings);
        SaveParametersConfig(configSettings);

        configSettings.BuildingNumberParam = settings.BuildingNumberParam?.Definition.Name;
        configSettings.ConstrWorksNumberParam = settings.ConstrWorksNumberParam?.Definition.Name;
        configSettings.ApartmentFullNumberParam = settings.ApartmentFullNumberParam?.Definition.Name;
        configSettings.ApartmentAreaCoefParam = settings.ApartmentAreaCoefParam?.Definition.Name;
        configSettings.ApartmentAreaLivingParam = settings.ApartmentAreaLivingParam?.Definition.Name;
        configSettings.ApartmentAreaNonSumParam = settings.ApartmentAreaNonSumParam?.Definition.Name;
        configSettings.RoomsAmountParam = settings.RoomsAmountParam?.Definition.Name;
        configSettings.RoomsHeightParam = settings.RoomsHeightParam?.Definition.Name;

        configSettings.RoomNumberParam = settings.RoomNumberParam?.Definition.Name;
        configSettings.RoomAreaCoefParam = _settings.RoomAreaCoefParam?.Definition.Name;

        configSettings.PrioritiesFilePath = _prioritiesViewModel.FilePath;

        config.SaveProjectConfig();
    }

    private void LoadConfig() {
        var config = ApartmentsConfig.GetPluginConfig();
        var configSettings = config.GetSettings(_revitRepository.Document);

        if(configSettings is null) {
            return;
        }

        LoadMainWindowConfig(configSettings);
        _parametersViewModel.SetParametersFromConfig(configSettings);

        if(!string.IsNullOrEmpty(configSettings.PrioritiesFilePath)) {
            _prioritiesViewModel.SetConfigFromPath(configSettings.PrioritiesFilePath);
        }

        config.SaveProjectConfig();
    }

    private void SetApartSettings() {
        var apartParamsVM = (ApartmentsParamsVM) _parametersViewModel;
        _settings.ApartmentFullNumberParam = apartParamsVM.SelectedApartFullNumParam;
        _settings.BuildingNumberParam = apartParamsVM.SelectedBuildingNumberParam;
        _settings.ConstrWorksNumberParam = apartParamsVM.SelectedConstrWorksNumberParam;
        _settings.ApartmentAreaCoefParam = apartParamsVM.SelectedApartAreaCoefParam;
        _settings.ApartmentAreaLivingParam = apartParamsVM.SelectedApartAreaLivingParam;
        _settings.ApartmentAreaNonSumParam = apartParamsVM.SelectedApartAreaNonSumParam;
        _settings.RoomsAmountParam = apartParamsVM.SelectedRoomsAmountParam;
        _settings.RoomsHeightParam = apartParamsVM.SelectedRoomsHeightParam;
        _settings.RoomAreaCoefParam = apartParamsVM.SelectedRoomAreaCoefParam;
    }
}
