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
internal class ApartmentsMainVM : MainViewModel {
    private new readonly ApartmentsSettings _settings;
    
    public ApartmentsMainVM(RevitRepository revitRepository, 
                            ApartmentsSettings settings,
                            IMessageBoxService messageBoxService,
                            ErrorWindowService errorWindowService)
        : base(revitRepository, settings, messageBoxService, errorWindowService) {
        _settings = settings;

        _declarationViewModel = new DeclarationApartVM(_revitRepository, settings, messageBoxService);
        _parametersViewModel = new ApartmentsParamsVM(_revitRepository, this);
        _prioritiesViewModel = new PrioritiesViewModel(this, messageBoxService);

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
            .Where(x => x.Elements.Any());
        if(parameterErrors.Any()) {
            _errorWindowService.ShowNoticeWindow(parameterErrors.ToList());
            return;
        }

        var projects = checkedDocuments
            .Select(x => new ApartmentsProject(x, _revitRepository, _settings))
            .ToList();

        // Проверка 2. Наличие квартир на выбранной стадии во всех выбранных проектах.
        var noApartsErrors = projects
            .Select(x => x.CheckRoomGroupsInProject())
            .Where(x => x.Elements.Any());
        if(noApartsErrors.Any()) {
            _errorWindowService.ShowNoticeWindow(noApartsErrors.ToList());
            return;
        }

        // Проверка 3. У всех помещений каждой квартиры должны совпадать общие площади квартиры.
        var areasErrors = projects
            .Select(x => x.CheckRoomAreasEquality())
            .Where(x => x.Elements.Any());
        if(areasErrors.Any()) {
            _errorWindowService.ShowNoticeWindow(areasErrors.ToList());
            return;
        }

        // Проверка 4. У каждого помещения должны быть актуальные площади помещения из кватирографии.
        var actualRoomAreasErrors = projects
            .Select(x => x.CheckActualRoomAreas())
            .Where(x => x.Elements.Any());
        if(actualRoomAreasErrors.Any()) {
            bool windowResult = _errorWindowService.ShowNoticeWindow(actualRoomAreasErrors.ToList(), true);

            if(!windowResult) {
                return;
            }
        }

        // Проверка 5. У каждого помещения должны быть актуальные площади квартиры из кватирографии.
        var actualApartmentAreasErrors = projects
            .Select(x => x.CheckActualApartmentAreas())
            .Where(x => x.Elements.Any());
        if(actualApartmentAreasErrors.Any()) {
            bool windowResult = _errorWindowService.ShowNoticeWindow(actualApartmentAreasErrors.ToList(), true);

            if(!windowResult) {
                return;
            }
        }

        if(_declarationViewModel.LoadUtp) {
            // Проверка 6. Проверка проекта для корректной выгрузки УТП.
            var utpErrors = projects
                .Select(x => x.CheckUtpWarnings())
                .SelectMany(x => x)
                .Where(x => x.Elements.Any());
            if(utpErrors.Any()) {
                bool windowResult = _errorWindowService.ShowNoticeWindow(utpErrors.ToList(), true);

                if(!windowResult) {
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
            var messageBoxResult = _messageBoxService.Show(
                $"Произошла ошибка выгрузки: {e.Message}.\n\nПопробовать выгрузить декларацию в формате csv?",
                "Ошибка выгрузки",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if(messageBoxResult == MessageBoxResult.Yes) {
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
