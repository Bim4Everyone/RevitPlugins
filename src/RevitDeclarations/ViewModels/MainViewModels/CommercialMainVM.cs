using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitDeclarations.Models;
using RevitDeclarations.Views;

using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using TaskDialogResult = Autodesk.Revit.UI.TaskDialogResult;

namespace RevitDeclarations.ViewModels {
    internal class CommercialMainVM : MainViewModel {
        private readonly CommercialExcelExportVM _excelExportViewModel;
        private readonly CommercialCsvExportVM _csvExportViewModel;

        public CommercialMainVM(RevitRepository revitRepository, DeclarationSettings settings) 
            : base(revitRepository, settings) {
            _excelExportViewModel =
                new CommercialExcelExportVM("Excel", new Guid("8D69848F-159D-4B26-B4C0-17E3B3A132CC"), _settings);
            _csvExportViewModel =
                new CommercialCsvExportVM("csv", new Guid("EC72C14A-9D4A-4D8B-BD35-50801CE68C24"), _settings);

            _exportFormats = new List<ExportViewModel>() {
                _excelExportViewModel,
                _csvExportViewModel
            };
            _selectedFormat = _exportFormats[0];

            // Нежилое помещение,Машино-место,Кладовая
            _parametersViewModel = new ParametersViewModel(_revitRepository, this);
            _prioritiesViewModel = new PrioritiesViewModel(this);

            _loadUtp = false;
            _canLoadUtp = false;

            LoadConfig();
        }

        public override void ExportDeclaration(object obj) {
            SetSelectedSettings();

            List<RevitDocumentViewModel> checkedDocuments = _revitDocuments
                .Where(x => x.IsChecked)
                .ToList();

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
                .Select(x => x.CheckRoomGroupsInRpoject())
                .Where(x => x.Errors.Any());
            if(noApartsErrors.Any()) {
                var window = new ErrorWindow() { DataContext = new ErrorsViewModel(noApartsErrors, false) };
                window.ShowDialog();
                return;
            }

            List<CommercialRooms> commercialRooms = projects
                .SelectMany(x => x.RoomGroups)
                .Cast<CommercialRooms>()
                .OrderBy(x => x.Number)
                .ThenBy(x => ValueConverter.ConvertStringToInt(x.Rooms.First().Number))
                .ToList();

            try {
                _selectedFormat.Export(FullPath, commercialRooms);
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
            var config = PluginConfig.GetPluginConfig();

            var configSettings =
                config.GetSettings(_revitRepository.Document) ?? config.AddSettings(_revitRepository.Document);

            configSettings.DeclarationName = FileName;
            configSettings.DeclarationPath = FilePath;
            configSettings.ExportFormat = SelectedFormat.Id;
            configSettings.Phase = SelectedPhase.Name;

            configSettings.RevitDocuments = RevitDocuments
                .Where(x => x.IsChecked)
                .Select(x => x.Name)
                .ToList();

            configSettings.FilterRoomsParam = _settings.FilterRoomsParam?.Definition.Name;
            configSettings.FilterRoomsValue = _settings.FilterRoomsValue;
            configSettings.GroupingBySectionParam = _settings.GroupingBySectionParam?.Definition.Name;
            configSettings.GroupingByGroupParam = _settings.GroupingByGroupParam?.Definition.Name;
            configSettings.MultiStoreyParam = _settings.MultiStoreyParam?.Definition.Name;

            configSettings.ApartmentFullNumberParam = _settings.ApartmentFullNumberParam?.Definition.Name;
            configSettings.DepartmentParam = _settings.DepartmentParam?.Definition.Name;
            configSettings.LevelParam = _settings.LevelParam?.Definition.Name;
            configSettings.SectionParam = _settings.SectionParam?.Definition.Name;
            configSettings.BuildingParam = _settings.BuildingParam?.Definition.Name;
            configSettings.ApartmentNumberParam = _settings.ApartmentNumberParam?.Definition.Name;
            configSettings.ApartmentAreaParam = _settings.ApartmentAreaParam?.Definition.Name;
            configSettings.ApartmentAreaCoefParam = _settings.ApartmentAreaCoefParam?.Definition.Name;
            configSettings.ApartmentAreaLivingParam = _settings.ApartmentAreaLivingParam?.Definition.Name;
            configSettings.RoomsAmountParam = _settings.RoomsAmountParam?.Definition.Name;
            configSettings.ProjectNameID = _settings.ProjectName;
            configSettings.ApartmentAreaNonSumParam = _settings.ApartmentAreaNonSumParam?.Definition.Name;
            configSettings.RoomsHeightParam = _settings.RoomsHeightParam?.Definition.Name;

            configSettings.RoomAreaParam = _settings.RoomAreaParam?.Definition.Name;
            configSettings.RoomAreaCoefParam = _settings.RoomAreaCoefParam?.Definition.Name;

            configSettings.GroupNameParam = _settings.GroupNameParam?.Definition.Name;

            config.SaveProjectConfig();
        }

        private void LoadConfig() {
            var config = PluginConfig.GetPluginConfig();
            var configSettings = config.GetSettings(_revitRepository.Document);

            if(configSettings is null)
                return;

            FileName = configSettings.DeclarationName;
            FilePath = configSettings.DeclarationPath;
            SelectedFormat = ExportFormats
                .FirstOrDefault(x => x.Id == configSettings.ExportFormat) ?? _exportFormats.FirstOrDefault();
            SelectedPhase = Phases
                .FirstOrDefault(x => x.Name == configSettings.Phase) ?? _phases[_phases.Count - 1];


            foreach(var document in RevitDocuments.Where(x => configSettings.RevitDocuments.Contains(x.Name))) {
                document.IsChecked = true;
            }

            config.SaveProjectConfig();
        }
    }
}
