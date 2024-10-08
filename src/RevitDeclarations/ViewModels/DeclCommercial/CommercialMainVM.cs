using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.WindowsAPICodePack.Dialogs;

using RevitDeclarations.Models;
using RevitDeclarations.Views;

using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using TaskDialogResult = Autodesk.Revit.UI.TaskDialogResult;

namespace RevitDeclarations.ViewModels {
    internal class CommercialMainVM : MainViewModel {
        //private readonly RevitRepository _revitRepository;
        //private readonly DeclarationSettings _settings;

        //private readonly ParametersViewModel _parametersViewModel;
        //private readonly PrioritiesViewModel _prioritiesViewModel;

        private readonly CommercialExcelExportVM _excelExportViewModel;
        private readonly ApartCsvExportVM _csvExportViewModel;
        private readonly List<ExportViewModel> _exportFormats;

        //private readonly IList<RevitDocumentViewModel> _revitDocuments;
        private readonly IReadOnlyList<Phase> _phases;

        private string _filePath;
        private string _fileName;
        private Phase _selectedPhase;
        private ExportViewModel _selectedFormat;
        private string _accuracy;
        private bool _loadUtp;
        private bool _canLoadUtp;
        private string _errorText;
        private string _canLoadUtpText;

        public CommercialMainVM(RevitRepository revitRepository, DeclarationSettings settings) 
            : base(revitRepository, settings) {
            //_revitRepository = revitRepository;
            //_settings = settings;

            _phases = _revitRepository.GetPhases();

            _excelExportViewModel =
                new CommercialExcelExportVM("Excel", new Guid("01EE33B6-69E1-4364-92FD-A2F94F115A9E"), _settings);
            _csvExportViewModel =
                new ApartCsvExportVM("csv", new Guid("BF1869ED-C5C4-4FCE-9DA9-F8F75A6B190D"), _settings);

            _exportFormats = new List<ExportViewModel>() {
                _excelExportViewModel,
                _csvExportViewModel
            };

            _accuracy = "1";
            _loadUtp = true;
            _canLoadUtp = true;

            //_revitDocuments = _revitRepository
            //    .GetLinks()
            //    .Select(x => new RevitDocumentViewModel(x, _settings))
            //    .Where(x => x.HasRooms())
            //    .OrderBy(x => x.Name)
            //    .ToList();

            RevitDocumentViewModel currentDocumentVM =
                new RevitDocumentViewModel(_revitRepository.Document, _settings);

            if(currentDocumentVM.HasRooms()) {
                _revitDocuments.Insert(0, currentDocumentVM);
            }

            //_parametersViewModel = new ParametersViewModel(_revitRepository, this);
            //_prioritiesViewModel = new PrioritiesViewModel(this);

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

        public IReadOnlyList<ExportViewModel> ExportFormats => _exportFormats;
        public ExportViewModel SelectedFormat {
            get => _selectedFormat;
            set => RaiseAndSetIfChanged(ref _selectedFormat, value);
        }

        //public IList<RevitDocumentViewModel> RevitDocuments => _revitDocuments;
        //public ParametersViewModel ParametersViewModel => _parametersViewModel;
        //public PrioritiesViewModel PrioritiesViewModel => _prioritiesViewModel;

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

            ParametersViewModel paramVM = new ParametersViewModel(_revitRepository, this);
            paramVM.SetCompanyParamConfig(obj);

            paramVM.FilterRoomsValue = "Нежилое помещение";

            _settings.ParametersVM = paramVM;
            _settings.PrioritiesConfig = new PrioritiesConfig();
            _settings.LoadUtp = _loadUtp;

            List<RevitDocumentViewModel> checkedDocuments = _revitDocuments
                .Where(x => x.IsChecked)
                .ToList();

            List<CommercialProject> projects = checkedDocuments
                .Select(x => new CommercialProject(x, _revitRepository, _settings))
                .ToList();

            List<CommercialRooms> commercialRooms = projects
                .SelectMany(x => x.CommercialRooms)
                .OrderBy(x => x.Section)
                .ToList();

            TaskDialog.Show("title", commercialRooms.ToString());

            _selectedFormat.Export(FullPath, commercialRooms);
            //try {
            //} catch(Exception e) {
            //    var taskDialog = new TaskDialog("Ошибка выгрузки") {
            //        CommonButtons = TaskDialogCommonButtons.No | TaskDialogCommonButtons.Yes,
            //        MainContent = "Произошла ошибка выгрузки.\nПопробовать выгрузить декларацию в формате csv?",
            //        ExpandedContent = $"Описание ошибки: {e.Message}"
            //    };

            //    TaskDialogResult dialogResult = taskDialog.Show();

            //    if(dialogResult == TaskDialogResult.Yes) {
            //        _csvExportViewModel.Export(FullPath, commercialRooms);
            //    }
            //}
        }

        public bool CanExport(object obj) {
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
