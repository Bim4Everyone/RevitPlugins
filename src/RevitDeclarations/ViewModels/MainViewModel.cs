using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.WindowsAPICodePack.Dialogs;

using RevitDeclarations.Models;
using RevitDeclarations.Views;

namespace RevitDeclarations.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly DeclarationSettings _settings;

        private readonly ParametersViewModel _parametersViewModel;
        private readonly PrioritiesViewModel _prioritiesViewModel;
        private readonly IList<RevitDocumentViewModel> _revitDocuments;
        private readonly List<Phase> _phases;

        private string _filePath;
        private string _fileName;
        private string _accuracy;
        private Phase _selectedPhase;
        private string _errorText;

        public MainViewModel(RevitRepository revitRepository, DeclarationSettings settings) {
            _revitRepository = revitRepository;
            _settings = settings;

            _phases = _revitRepository.GetPhases();
            _selectedPhase = _phases[_phases.Count - 1];

            _accuracy = "1";

            _revitDocuments = _revitRepository
                .GetLinks()
                .Select(x => new RevitDocumentViewModel(x, _settings))
                .Where(x => x.HasRooms())
                .ToList();

            RevitDocumentViewModel currentDocumentVM = 
                new RevitDocumentViewModel(_revitRepository.Document, _settings);

            if(currentDocumentVM.HasRooms()) {
                _revitDocuments.Insert(0, currentDocumentVM);
            }

            _parametersViewModel = new ParametersViewModel(_revitRepository, this);
            _prioritiesViewModel = new PrioritiesViewModel(_settings.Priorities);

            SelectFolderCommand = new RelayCommand(SelectFolder);
            ExportToExcelCommand = new RelayCommand(ExportToExcel, CanExportToExcel);

            LoadConfig();
        }

        public ICommand SelectFolderCommand { get; }
        public ICommand ExportToExcelCommand { get; }

        public string FilePath {
            get => _filePath;
            set => RaiseAndSetIfChanged(ref _filePath, value);
        }

        public string FileName {
            get => _fileName;
            set => RaiseAndSetIfChanged(ref _fileName, value);
        }

        public string FullPath => FilePath + "\\" + FileName;

        public List<Phase> Phases => _phases;
        public Phase SelectedPhase {
            get => _selectedPhase;
            set => RaiseAndSetIfChanged(ref _selectedPhase, value);
        }

        public string Accuracy {
            get => _accuracy;
            set => RaiseAndSetIfChanged(ref _accuracy, value);
        }

        public IList<RevitDocumentViewModel> RevitDocuments => _revitDocuments;
        public ParametersViewModel ParametersViewModel => _parametersViewModel;
        public PrioritiesViewModel PrioritiesViewModel => _prioritiesViewModel;

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        public void SelectFolder(object obj) {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog() {
                IsFolderPicker = true
            };

            if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                FilePath = dialog.FileName;
            }
        }

        public void ExportToExcel(object obj) {
            int.TryParse(_accuracy, out int accuracy);
            _settings.Accuracy = accuracy;
            _settings.SelectedPhase = _selectedPhase;
            _settings.ViewModel = ParametersViewModel;

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

            List<DeclarationProject> projects = checkedDocuments
                .Select(x => new DeclarationProject(x, _revitRepository, _settings))
                .ToList();

            // Проверка 2. Наличие квартир на выбранной стадии во всех выбранных проектах.
            IEnumerable<ErrorsListViewModel> noApartsErrors = projects
                .Select(x => x.CheckApartmentsInRpoject())
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

            ExportProjects(projects);
        }

        private void ExportProjects(List<DeclarationProject> projects) {
            foreach(DeclarationProject project in projects) {
                project.CalculateUtpForApartments();
            }

            List<Apartment> apartments = projects
                .SelectMany(x => x.Apartments)
                .ToList();

            DeclarationTableData tableData = new DeclarationTableData(apartments, _settings);
            DeclarationExporter exporter = new DeclarationExporter(tableData, _settings);
            exporter.ExportToExcel(FullPath);
        }

        public bool CanExportToExcel(object obj) {
            IEnumerable<RevitDocumentViewModel> checkedDocuments = _revitDocuments
                .Where(x => x.IsChecked);

            bool hasCheckedDocuments = _revitDocuments
                .Where(x => x.IsChecked)
                .Any();

            bool hasPhases = checkedDocuments
                .All(x => x.HasPhase(_selectedPhase));

            if(string.IsNullOrEmpty(_filePath)) {
                ErrorText = "Не выбрана папка";
                return false;
            }
            if(string.IsNullOrEmpty(_fileName)) {
                ErrorText = "Не заполнено имя файла";
                return false;
            }
            if(!hasCheckedDocuments) {
                ErrorText = "Не выбраны проекты для выгрузки";
                return false;
            }
            if(!hasPhases) {
                ErrorText = "В выбранных проектах отсутствует выбранная стадия";
                return false;
            }

            ErrorText = "";
            return true;
        }

        private void SaveConfig() {
            var config = PluginConfig.GetPluginConfig();

            var configSettings =
                config.GetSettings(_revitRepository.Document) ?? config.AddSettings(_revitRepository.Document);

            configSettings.DeclarationName = FileName;
            configSettings.DeclarationPath = FilePath;
            configSettings.Phase = SelectedPhase.Name;
            configSettings.Accuracy = Accuracy;

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

            config.SaveProjectConfig();
        }

        private void LoadConfig() {
            var config = PluginConfig.GetPluginConfig();
            var configSettings = config.GetSettings(_revitRepository.Document);

            if(configSettings is null)
                return;

            FileName = configSettings.DeclarationName;
            FilePath = configSettings.DeclarationPath;
            SelectedPhase = Phases.FirstOrDefault(x => x.Name == configSettings.Phase);
            Accuracy = configSettings.Accuracy;

            if(SelectedPhase == null) {
                SelectedPhase = _selectedPhase = _phases[_phases.Count - 1];
            }

            _parametersViewModel.SetParametersFromConfig(configSettings);

            config.SaveProjectConfig();
        }
    }
}
