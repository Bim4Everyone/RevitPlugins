using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitDeclarations.Models;
using RevitDeclarations.Models.Configs;
using RevitDeclarations.Views;

using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using TaskDialogResult = Autodesk.Revit.UI.TaskDialogResult;

namespace RevitDeclarations.ViewModels {
    internal class CommercialMainVM : MainViewModel {
        private readonly CommercialExcelExportVM _excelExportViewModel;
        private readonly CommercialCsvExportVM _csvExportViewModel;

        private new readonly CommercialSettings _settings;

        public CommercialMainVM(RevitRepository revitRepository, CommercialSettings settings) 
            : base(revitRepository, settings) {
            _settings = settings;

            _excelExportViewModel =
                new CommercialExcelExportVM("Excel", new Guid("8D69848F-159D-4B26-B4C0-17E3B3A132CC"), _settings);
            _csvExportViewModel =
                new CommercialCsvExportVM("csv", new Guid("EC72C14A-9D4A-4D8B-BD35-50801CE68C24"), _settings);

            _exportFormats = new List<ExportViewModel>() {
                _excelExportViewModel,
                _csvExportViewModel
            };
            _selectedFormat = _exportFormats[0];

            _parametersViewModel = new CommercialParamsVM(_revitRepository, this);
            _prioritiesViewModel = new PrioritiesViewModel(this);

            _loadUtp = false;
            _canLoadUtp = false;

            LoadConfig();
        }

        public override void ExportDeclaration(object obj) {
            SetSelectedSettings();
            SetCommSettings();

            List<RevitDocumentViewModel> checkedDocuments = _revitDocuments
                .Where(x => x.IsChecked)
                .ToList();

            SaveConfig();

            // Проверка 1. Наличие параметров во всех выбранных проектах.
            IEnumerable<ErrorsListViewModel> parameterErrors = checkedDocuments
                .Select(x => x.CheckParameters())
                .Where(x => x.Errors.Any());
            if(parameterErrors.Any()) {
                var window = new ErrorWindow() { DataContext = new ErrorsViewModel(parameterErrors, false) };
                window.ShowDialog();
                return;
            }

            List<CommercialProject> projects = checkedDocuments
                .Select(x => new CommercialProject(x, _revitRepository, _settings))
                .ToList();

            // Проверка 2. Наличие групп помещений на выбранной стадии во всех выбранных проектах.
            IEnumerable<ErrorsListViewModel> noApartsErrors = projects
                .Select(x => x.CheckRoomGroupsInProject())
                .Where(x => x.Errors.Any());
            if(noApartsErrors.Any()) {
                var window = new ErrorWindow() { DataContext = new ErrorsViewModel(noApartsErrors, false) };
                window.ShowDialog();
                return;
            }

            // Проверка 3. У каждого помещения должны быть актуальные площади помещения из кватирографии.
            IEnumerable<ErrorsListViewModel> actualRoomAreasErrors = projects
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

            List<CommercialRooms> commercialRooms = projects
                .SelectMany(x => x.RoomGroups)
                .Cast<CommercialRooms>()
                .OrderBy(x => x.Number ?? "", _stringComparer)
                .ThenBy(x => x.Rooms.First().Number ?? "", _stringComparer)
                .ToList();

            _selectedFormat.Export(FullPath, commercialRooms);
            try {
            } catch(Exception e) {
                var taskDialog = new TaskDialog("Ошибка выгрузки") {
                    CommonButtons = TaskDialogCommonButtons.No | TaskDialogCommonButtons.Yes,
                    MainContent = "Произошла ошибка выгрузки.\nПопробовать выгрузить декларацию в формате csv?",
                    ExpandedContent = $"Описание ошибки: {e.Message}"
                };

                TaskDialogResult dialogResult = taskDialog.Show();

                if(dialogResult == TaskDialogResult.Yes) {
                    _csvExportViewModel.Export(FullPath, commercialRooms);
                }
            }
        }

        private void SaveConfig() {
            CommercialSettings settings = (CommercialSettings) _settings;

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

            if(configSettings is null)
                return;

            LoadMainWindowConfig(configSettings);
            _parametersViewModel.SetParametersFromConfig(configSettings);

            config.SaveProjectConfig();
        }


        private void SetCommSettings() {
            CommercialParamsVM commercialParamsVM = (CommercialParamsVM) _parametersViewModel;
            _settings.BuildingNumberParam = commercialParamsVM.SelectedBuildingNumberParam;
            _settings.ConstrWorksNumberParam = commercialParamsVM.SelectedConstrWorksNumberParam;
            _settings.RoomsHeightParam = commercialParamsVM.SelectedRoomsHeightParam;
            _settings.ParkingSpaceClass = commercialParamsVM.SelectedParkingSpaceClass;
            _settings.ParkingInfo = commercialParamsVM.SelectedParkingInfo;
            _settings.GroupNameParam = commercialParamsVM.SelectedGroupNameParam;
            _settings.AddPrefixToNumber = commercialParamsVM.AddPrefixToNumber;
        }
    }
}
