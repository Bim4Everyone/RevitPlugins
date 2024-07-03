using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Windows.Forms;
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
        private readonly IReadOnlyList<Phase> _phases;

        private string _filePath;
        private string _fileName;
        private bool _exportToExcel;
        private string _accuracy;
        private Phase _selectedPhase;
        private bool _loadUtp;
        private bool _canLoadUtp;
        private string _errorText;
        private string _canLoadUtpText;

        public MainViewModel(RevitRepository revitRepository, DeclarationSettings settings) {
            _revitRepository = revitRepository;
            _settings = settings;

            _phases = _revitRepository.GetPhases();
            _selectedPhase = _phases[_phases.Count - 1];

            _accuracy = "1";
            _exportToExcel = true;
            _loadUtp = true;
            _canLoadUtp = true;

            _revitDocuments = _revitRepository
                .GetLinks()
                .Select(x => new RevitDocumentViewModel(x, _settings))
                .Where(x => x.HasRooms())
                .OrderBy(x => x.Name)
                .ToList();

            RevitDocumentViewModel currentDocumentVM = 
                new RevitDocumentViewModel(_revitRepository.Document, _settings);

            if(currentDocumentVM.HasRooms()) {
                _revitDocuments.Insert(0, currentDocumentVM);
            }

            _parametersViewModel = new ParametersViewModel(_revitRepository, this);
            _prioritiesViewModel = new PrioritiesViewModel(this);

            SelectFolderCommand = new RelayCommand(SelectFolder);
            ExportDeclarationCommand = new RelayCommand(ExportDeclaration, CanExport);

            LoadConfig();
        }

        public ICommand SelectFolderCommand { get; }
        public ICommand ExportDeclarationCommand { get; }

        public string FilePath {
            get => _filePath;
            set => RaiseAndSetIfChanged(ref _filePath, value);
        }
        public string FileName {
            get => _fileName;
            set => RaiseAndSetIfChanged(ref _fileName, value);
        }
        public string FullPath => FilePath + "\\" + FileName;
        public bool ExportToExcel {
            get => _exportToExcel;
            set => RaiseAndSetIfChanged(ref _exportToExcel, value);
        }

        public IReadOnlyList<Phase> Phases => _phases;
        public Phase SelectedPhase {
            get => _selectedPhase;
            set => RaiseAndSetIfChanged(ref _selectedPhase, value);
        }

        public string Accuracy {
            get => _accuracy;
            set => RaiseAndSetIfChanged(ref _accuracy, value);
        }

        public bool LoadUtp {
            get => _loadUtp;
            set => RaiseAndSetIfChanged(ref _loadUtp, value);
        }

        public bool CanLoadUtp {
            get => _canLoadUtp;
            set => RaiseAndSetIfChanged(ref _canLoadUtp, value);
        }

        public IList<RevitDocumentViewModel> RevitDocuments => _revitDocuments;
        public ParametersViewModel ParametersViewModel => _parametersViewModel;
        public PrioritiesViewModel PrioritiesViewModel => _prioritiesViewModel;

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string CanLoadUtpText {
            get => _canLoadUtpText;
            set => RaiseAndSetIfChanged(ref _canLoadUtpText, value);
        }

        public void SelectFolder(object obj) {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog() {
                IsFolderPicker = true
            };

            if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                FilePath = dialog.FileName;
            }
        }

        public void ExportDeclaration(object obj) {
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

                foreach(DeclarationProject project in projects) {
                    project.CalculateUtpForApartments();
                }
            }

            List<Apartment> apartments = projects
                .SelectMany(x => x.Apartments)
                .OrderBy(x => x.Building)
                .ThenBy(x => x.FullNumber)
                .ToList();

            DeclarationExporter exporter = new DeclarationExporter(_settings);

            if(ExportToExcel) {
                ExcelTableData tableData = new ExcelTableData(apartments, _settings);
                exporter.ExportToExcel(FullPath, tableData);
            } else {
                exporter.ExportToJson(FullPath, apartments);
            }
        }

        public bool CanExport(object obj) {
            IEnumerable<RevitDocumentViewModel> checkedDocuments = _revitDocuments
                .Where(x => x.IsChecked);

            bool hasCheckedDocuments = _revitDocuments
                .Where(x => x.IsChecked)
                .Any();

            bool hasPhases = checkedDocuments
                .All(x => x.HasPhase(_selectedPhase));

            bool hasEmptyParameters = _parametersViewModel
                .GetAllParametrs()
                .Where(x => x == null)
                .Any();

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
            if(hasEmptyParameters 
                || string.IsNullOrEmpty(_parametersViewModel.FilterRoomsValue)
                || string.IsNullOrEmpty(_parametersViewModel.ProjectName)) {
                ErrorText = "Не выбран параметр на вкладке \"Параметры\"";
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
            configSettings.ExportToExcel = ExportToExcel;
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
            ExportToExcel = configSettings.ExportToExcel;
            SelectedPhase = Phases.FirstOrDefault(x => x.Name == configSettings.Phase);

            if(SelectedPhase == null) {
                SelectedPhase = _selectedPhase = _phases[_phases.Count - 1];
            }

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
