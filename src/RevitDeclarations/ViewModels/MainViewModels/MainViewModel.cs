using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.WindowsAPICodePack.Dialogs;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal abstract class MainViewModel : BaseViewModel {
        private protected readonly RevitRepository _revitRepository;
        private protected readonly DeclarationSettings _settings;

        private protected readonly IList<RevitDocumentViewModel> _revitDocuments;
        private protected readonly IReadOnlyList<Phase> _phases;
        private protected Phase _selectedPhase;

        private protected string _filePath;
        private protected string _fileName;

        private protected ParametersViewModel _parametersViewModel;
        private protected PrioritiesViewModel _prioritiesViewModel;

        private protected List<ExportViewModel> _exportFormats;
        private protected ExportViewModel _selectedFormat;

        private protected string _accuracy;

        private protected bool _loadUtp;
        private protected bool _canLoadUtp;
        private protected string _canLoadUtpText;

        private string _errorText;

        public MainViewModel(RevitRepository revitRepository, DeclarationSettings settings) {
            _revitRepository = revitRepository;
            _settings = settings;

            _phases = _revitRepository.GetPhases();
            _selectedPhase = _phases[_phases.Count() - 1];

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

            _accuracy = "1";

            SelectFolderCommand = new RelayCommand(SelectFolder);
            ExportDeclarationCommand = new RelayCommand(ExportDeclaration, CanExport);
        }

        public ICommand SelectFolderCommand { get; }
        public ICommand ExportDeclarationCommand { get; }

        public ParametersViewModel ParametersViewModel => _parametersViewModel;
        public PrioritiesViewModel PrioritiesViewModel => _prioritiesViewModel;

        public IList<RevitDocumentViewModel> RevitDocuments => _revitDocuments;
        public IReadOnlyList<Phase> Phases => _phases;

        public Phase SelectedPhase {
            get => _selectedPhase;
            set => RaiseAndSetIfChanged(ref _selectedPhase, value);
        }

        public string FilePath {
            get => _filePath;
            set => RaiseAndSetIfChanged(ref _filePath, value);
        }
        public string FileName {
            get => _fileName;
            set => RaiseAndSetIfChanged(ref _fileName, value);
        }
        public string FullPath => FilePath + "\\" + FileName;

        public IReadOnlyList<ExportViewModel> ExportFormats => _exportFormats;
        public ExportViewModel SelectedFormat {
            get => _selectedFormat;
            set => RaiseAndSetIfChanged(ref _selectedFormat, value);
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
        public string CanLoadUtpText {
            get => _canLoadUtpText;
            set => RaiseAndSetIfChanged(ref _canLoadUtpText, value);
        }

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

        public virtual void ExportDeclaration(object obj) { }

        public bool CanExport(object obj) {
            IEnumerable<RevitDocumentViewModel> checkedDocuments = _revitDocuments
                .Where(x => x.IsChecked);

            bool hasCheckedDocuments = _revitDocuments
                .Where(x => x.IsChecked)
                .Any();

            bool hasPhases = checkedDocuments
                .All(x => x.HasPhase(_selectedPhase));

            bool hasEmptyParameters = _parametersViewModel
                .AllSelectedParameters
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
            if(hasEmptyParameters) {
                ErrorText = "Не выбран параметр на вкладке \"Параметры\"";
                return false;
            }
            if(!_parametersViewModel.FilterRoomsValues.Any()) {
                ErrorText = "Не заполнены значения для фильтрации на вкладке \"Параметры\"";
                return false;
            }
            if(string.IsNullOrEmpty(_parametersViewModel.ProjectName)) {
                ErrorText = "Не заполнено ИД объекта на вкладке \"Параметры\"";
                return false;
            }

            ErrorText = "";
            return true;
        }

        public void SetSelectedSettings() {
            int.TryParse(_accuracy, out int accuracy);
            _settings.Accuracy = accuracy;
            _settings.SelectedPhase = _selectedPhase;

            _settings.ParametersVM = _parametersViewModel;
            _settings.PrioritiesConfig = _prioritiesViewModel.PrioritiesConfig;

            _settings.LoadUtp = _loadUtp;
        }

        /// <summary>
        /// Сохранение настроек вкладки DeclarationTabItem.
        /// Эти настройки одинаковы для всех деклараций.
        /// </summary>
        public void SaveMainWindowConfig(DeclarationConfigSettings configSettings) {
            configSettings.DeclarationName = FileName;
            configSettings.DeclarationPath = FilePath;
            configSettings.ExportFormat = SelectedFormat.Id;
            configSettings.Phase = SelectedPhase.Name;

            configSettings.RevitDocuments = RevitDocuments
                .Where(x => x.IsChecked)
                .Select(x => x.Name)
                .ToList();
        }

        /// <summary>
        /// Сохранение настроек вкладки ParamsTabItem.
        /// Сохраняются настройки, общие для всех деклараций.
        /// </summary>
        public void SaveParametersConfig(DeclarationConfigSettings configSettings) {
            configSettings.FilterRoomsParam = _settings.FilterRoomsParam?.Definition.Name;
            configSettings.FilterRoomsValues = _settings.FilterRoomsValues;
            configSettings.GroupingBySectionParam = _settings.GroupingBySectionParam?.Definition.Name;
            configSettings.GroupingByGroupParam = _settings.GroupingByGroupParam?.Definition.Name;
            configSettings.MultiStoreyParam = _settings.MultiStoreyParam?.Definition.Name;

            configSettings.DepartmentParam = _settings.DepartmentParam?.Definition.Name;
            configSettings.LevelParam = _settings.LevelParam?.Definition.Name;
            configSettings.SectionParam = _settings.SectionParam?.Definition.Name;
            configSettings.BuildingParam = _settings.BuildingParam?.Definition.Name;

            configSettings.ApartmentNumberParam = _settings.ApartmentNumberParam?.Definition.Name;
            configSettings.ApartmentAreaParam = _settings.ApartmentAreaParam?.Definition.Name;

            configSettings.RoomAreaParam = _settings.RoomAreaParam?.Definition.Name;
            configSettings.RoomNameParam = _settings.RoomNameParam?.Definition.Name;

            configSettings.ProjectNameID = _settings.ProjectName;
        }

        public void LoadMainWindowConfig(DeclarationConfigSettings configSettings) {
            FileName = configSettings.DeclarationName;
            FilePath = configSettings.DeclarationPath;
            SelectedFormat = ExportFormats
                .FirstOrDefault(x => x.Id == configSettings.ExportFormat) ?? _exportFormats.FirstOrDefault();
            SelectedPhase = Phases
                .FirstOrDefault(x => x.Name == configSettings.Phase) ?? _phases[_phases.Count - 1];

            var documents = RevitDocuments
                .Where(x => configSettings.RevitDocuments.Contains(x.Name));

            foreach(var document in documents) {
                document.IsChecked = true;
            }
        }
    }
}