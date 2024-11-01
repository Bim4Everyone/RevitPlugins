using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitDeclarations.Models;
using RevitDeclarations.ViewModels.ExportViewModels;
using RevitDeclarations.Views;

using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using TaskDialogResult = Autodesk.Revit.UI.TaskDialogResult;

namespace RevitDeclarations.ViewModels {
    internal class PublicAreasMainVM : MainViewModel {
        private readonly PublicAreasExcelExportVM _excelExportViewModel;
        private readonly PublicAreasCsvExportVM _csvExportViewModel;

        public PublicAreasMainVM(RevitRepository revitRepository, PublicAreasSettings settings)
            : base(revitRepository, settings) {
            _excelExportViewModel =
                new PublicAreasExcelExportVM("Excel", new Guid("186F3EEE-303A-42DF-910E-475AD2525ABD"), _settings);
            _csvExportViewModel =
                new PublicAreasCsvExportVM("csv", new Guid("A674AB16-642A-4642-BE51-51B812378734"), _settings);

            _exportFormats = new List<ExportViewModel>() {
                _excelExportViewModel,
                _csvExportViewModel
            };
            _selectedFormat = _exportFormats[0];

            _parametersViewModel = new PublicAreasParamsVM(_revitRepository, this);            
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

            List<PublicAreasProject> projects = checkedDocuments
                .Select(x => new PublicAreasProject(x, _revitRepository, _settings))
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

            List<PublicArea> commercialRooms = projects
                .SelectMany(x => x.RoomGroups)
                .Cast<PublicArea>()
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

        private void LoadConfig() {
            var config = CommercialConfig.GetPluginConfig();
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
