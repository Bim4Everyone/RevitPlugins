using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitDeclarations.Models;
using RevitDeclarations.Views;

using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using TaskDialogResult = Autodesk.Revit.UI.TaskDialogResult;

namespace RevitDeclarations.ViewModels {
    internal class ApartMainVM : MainViewModel {
        private readonly ParametersViewModel _parametersViewModel;
        private readonly PrioritiesViewModel _prioritiesViewModel;

        private readonly ApartExcelExportVM _excelExportViewModel;
        private readonly ApartCsvExportVM _csvExportViewModel;
        private readonly ApartJsonExportVM _jsonExportViewModel;
        private readonly List<ExportViewModel> _exportFormats;

        private ExportViewModel _selectedFormat;

        private bool _loadUtp;
        private bool _canLoadUtp;
        private string _canLoadUtpText;

        public ApartMainVM(RevitRepository revitRepository, DeclarationSettings settings) 
            : base(revitRepository, settings) {
            _excelExportViewModel = 
                new ApartExcelExportVM("Excel", new Guid("01EE33B6-69E1-4364-92FD-A2F94F115A9E"), _settings);
            _csvExportViewModel = 
                new ApartCsvExportVM("csv", new Guid("BF1869ED-C5C4-4FCE-9DA9-F8F75A6B190D"), _settings);
            _jsonExportViewModel = 
                new ApartJsonExportVM("json", new Guid("159FA27A-06E7-4515-9221-0BAFC0008F21"), _settings);

            _exportFormats = new List<ExportViewModel>() {
                _excelExportViewModel,
                _csvExportViewModel,
                _jsonExportViewModel,
            };

            _accuracy = "1";
            _loadUtp = true;
            _canLoadUtp = true;

            RevitDocumentViewModel currentDocumentVM = 
                new RevitDocumentViewModel(_revitRepository.Document, _settings);

            if(currentDocumentVM.HasRooms()) {
                _revitDocuments.Insert(0, currentDocumentVM);
            }

            _parametersViewModel = new ParametersViewModel(_revitRepository, this);
            _prioritiesViewModel = new PrioritiesViewModel(this);

            _selectedPhase = _phases[0];
            _selectedFormat = _exportFormats[0];

            LoadConfig();
        }

        public IReadOnlyList<Phase> Phases => _phases;
        public Phase SelectedPhase {
            get => _selectedPhase;
            set => RaiseAndSetIfChanged(ref _selectedPhase, value);
        }

        public bool LoadUtp {
            get => _loadUtp;
            set => RaiseAndSetIfChanged(ref _loadUtp, value);
        }

        public bool CanLoadUtp {
            get => _canLoadUtp;
            set => RaiseAndSetIfChanged(ref _canLoadUtp, value);
        }

        public IReadOnlyList<ExportViewModel> ExportFormats => _exportFormats;
        public ExportViewModel SelectedFormat {
            get => _selectedFormat;
            set => RaiseAndSetIfChanged(ref _selectedFormat, value);
        }

        public ParametersViewModel ParametersViewModel => _parametersViewModel;
        public PrioritiesViewModel PrioritiesViewModel => _prioritiesViewModel;

        public string CanLoadUtpText {
            get => _canLoadUtpText;
            set => RaiseAndSetIfChanged(ref _canLoadUtpText, value);
        }

        public override void ExportDeclaration(object obj) {
            int.TryParse(_accuracy, out int accuracy);
            _settings.Accuracy = accuracy;
            _settings.SelectedPhase = _selectedPhase;
            _settings.ParametersVM = ParametersViewModel;
            _settings.PrioritiesConfig = _prioritiesViewModel.PrioritiesConfig;
            _settings.LoadUtp = _loadUtp;

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

            List<ApartmentProject> projects = checkedDocuments
                .Select(x => new ApartmentProject(x, _revitRepository, _settings))
                .ToList();

            // Проверка 2. Наличие квартир на выбранной стадии во всех выбранных проектах.
            IEnumerable<ErrorsListViewModel> noApartsErrors = projects
                .Select(x => x.CheckRoomGroupsInRpoject())
                .Where(x => x.Errors.Any());
            if(noApartsErrors.Any()) {
                var window = new ErrorWindow() { DataContext = new ErrorsViewModel(noApartsErrors, false) };
                window.ShowDialog();
                return;
            }

            // Проверка 3. У всех помещений каждой квартиры должны совпадать общие площади квартиры.
            IEnumerable<ErrorsListViewModel> areasErrors = projects
                .Select(x => x.CheckRoomAreasEquality())
                .Where(x => x.Errors.Any());
            if(areasErrors.Any()) {
                var window = new ErrorWindow() { DataContext = new ErrorsViewModel(areasErrors, false) };
                window.ShowDialog();
                return;
            }

            // Проверка 4. У каждого помещения должны быть актуальные площади помещения из кватирографии.
            IEnumerable<ErrorsListViewModel> actualRoomAreasErrors = projects
                .Select(x => x.CheckActualRoomAreas())
                .Where(x => x.Errors.Any());
            if(actualRoomAreasErrors.Any()) {
                var window = new ErrorWindow() { 
                    DataContext = new ErrorsViewModel(actualRoomAreasErrors, true) };
                window.ShowDialog();

                if(!(bool) window.DialogResult) {
                    return;
                }
            }

            // Проверка 5. У каждого помещения должны быть актуальные площади квартиры из кватирографии.
            IEnumerable<ErrorsListViewModel> actualApartmentAreasErrors = projects
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

            if(_loadUtp) {
                // Проверка 6. Проверка проекта для корректной выгрузки УТП.
                IEnumerable<ErrorsListViewModel> utpErrors = projects
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

                foreach(ApartmentProject project in projects) {
                    project.CalculateUtpForApartments();
                }
            }

            List<Apartment> apartments = projects
                .SelectMany(x => x.RoomGroups)
                .Cast<Apartment>()
                .OrderBy(x => x.Section)
                .ThenBy(x => ValueConverter.ConvertStringToInt(x.FullNumber))
                .ToList();

            try {
                _selectedFormat.Export(FullPath, apartments);
            } catch(Exception e) {
                var taskDialog = new TaskDialog("Ошибка выгрузки") {
                    CommonButtons = TaskDialogCommonButtons.No | TaskDialogCommonButtons.Yes,
                    MainContent = "Произошла ошибка выгрузки.\nПопробовать выгрузить декларацию в формате csv?",
                    ExpandedContent = $"Описание ошибки: {e.Message}"
                };

                TaskDialogResult dialogResult = taskDialog.Show();

                if(dialogResult == TaskDialogResult.Yes) {
                    _csvExportViewModel.Export(FullPath, apartments);
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

            configSettings.PrioritiesFilePath = _prioritiesViewModel.FilePath;

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

            _parametersViewModel.SetParametersFromConfig(configSettings);

            if(!string.IsNullOrEmpty(configSettings.PrioritiesFilePath)) {
                _prioritiesViewModel.SetConfigFromPath(configSettings.PrioritiesFilePath);
            }

            config.SaveProjectConfig();
        }
    }
}
