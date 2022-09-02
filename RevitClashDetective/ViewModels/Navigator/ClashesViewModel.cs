using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.RevitClashReport;

namespace RevitClashDetective.ViewModels.Navigator {
    internal class ClashesViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private bool _openFromClashDetector;
        private string _message;
        private string _selectedFile;
        private DispatcherTimer _timer;
        private List<ClashViewModel> _allClashes;
        private List<ClashViewModel> _clashes;

        public ClashesViewModel(RevitRepository revitRepository, string selectedFile = null) {
            _revitRepository = revitRepository;

            if(selectedFile == null) {
                InitializeFiles();
            } else {
                InitializeFiles(selectedFile);
            }

            InitializeClashesFromFile();
            InitializeTimer();

            SelectionChangedCommand = new RelayCommand(SelectionChanged);
            SelectClashCommand = new RelayCommand(SelectClash, CanSelectClash);
            SaveCommand = new RelayCommand(SaveConfig, CanSaveConfig);

            OpenClashDetectorCommand = new RelayCommand(OpenClashDetector, p => OpenFromClashDetector);
        }

        public ICommand SelectClashCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SelectionChangedCommand { get; }
        public ICommand OpenClashDetectorCommand { get; }

        public string[] FileNames { get; set; }

        public bool IsColumnVisible => FileNames != null;

        public bool OpenFromClashDetector {
            get => _openFromClashDetector;
            set => this.RaiseAndSetIfChanged(ref _openFromClashDetector, value);
        }

        public string SelectedFile {
            get => _selectedFile;
            set => this.RaiseAndSetIfChanged(ref _selectedFile, value);
        }

        public List<ClashViewModel> Clashes {
            get => _clashes;
            set => this.RaiseAndSetIfChanged(ref _clashes, value);
        }

        public string Message {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        private void InitializeFiles(string selectedFile) {
            var profilePath = RevitRepository.ProfilePath;
            FileNames = Directory.GetFiles(Path.Combine(profilePath, ModuleEnvironment.RevitVersion, nameof(RevitClashDetective), _revitRepository.GetObjectName()))
                .Select(path => Path.GetFileNameWithoutExtension(path))
                .Where(item => item.Equals(selectedFile, StringComparison.CurrentCultureIgnoreCase))
                .ToArray();
            SelectedFile = FileNames.FirstOrDefault();
        }

        private void InitializeFiles() {
            var profilePath = RevitRepository.ProfilePath;
            var path = Path.Combine(profilePath, ModuleEnvironment.RevitVersion, nameof(RevitClashDetective), _revitRepository.GetObjectName());
            if(Directory.Exists(path)) {
                FileNames = Directory.GetFiles(path)
                                   .Select(item => Path.GetFileNameWithoutExtension(item))
                                   .ToArray();
                SelectedFile = FileNames.FirstOrDefault();
            }
        }

        private void InitializeClashesFromFile() {
            var documentNames = _revitRepository.GetDocuments().Select(item => item.Title).ToList();
            if(SelectedFile != null) {
                var config = ClashesConfig.GetClashesConfig(_revitRepository.GetObjectName(), SelectedFile);
                _allClashes = config.Clashes.Select(item => new ClashViewModel(_revitRepository, item))
                                            .ToList();

                Clashes = _allClashes.Where(item => IsValid(documentNames, item))
                                     .ToList();
            }
        }

        private bool IsValid(List<string> documentNames, ClashViewModel clash) {
            var clashDocuments = new[] { clash.FirstDocumentName, clash.SecondDocumentName };
            var clashElements = new[] {_revitRepository.GetElement(clash.Clash.MainElement.DocumentName, clash.Clash.MainElement.Id),
                                       _revitRepository.GetElement(clash.Clash.OtherElement.DocumentName, clash.Clash.OtherElement.Id)};

            return clashDocuments.All(item => documentNames.Any(d => d.Contains(item))) && clashElements.Any(item => item != null);
        }

        private void SelectClash(object p) {
            var clash = (ClashViewModel) p;

            var elements = new[] {_revitRepository.GetElement(clash.Clash.MainElement.DocumentName, clash.Clash.MainElement.Id),
                                  _revitRepository.GetElement(clash.Clash.OtherElement.DocumentName, clash.Clash.OtherElement.Id)};
            _revitRepository.SelectAndShowElement(elements.Where(item => item != null));
        }

        private bool CanSelectClash(object p) {
            return p != null && p is ClashViewModel;
        }

        private void SaveConfig(object p) {
            var config = ClashesConfig.GetClashesConfig(_revitRepository.GetObjectName(), SelectedFile);

            var notValidClashes = _allClashes.Except(Clashes)
                                             .Select(item => item.GetClashModel());

            config.Clashes = Clashes.Select(item => item.GetClashModel())
                .Union(notValidClashes)
                .ToList();

            config.SaveProjectConfig();
            Message = "Файл успешно сохранен";
            RefreshMessage();
        }

        private bool CanSaveConfig(object p) {
            return SelectedFile != null;
        }

        private void SelectionChanged(object p) {
            InitializeClashesFromFile();
        }

        private void OpenClashDetector(object p) {
            void action() {
                var command = new DetectiveClashesCommand();
                command.ExecuteCommand(_revitRepository.UiApplication);
            }
            _revitRepository.DoAction(action);
        }

        private void InitializeTimer() {
            _timer = new DispatcherTimer {
                Interval = new TimeSpan(0, 0, 0, 3)
            };
            _timer.Tick += (s, a) => { Message = null; _timer.Stop(); };
        }

        private void RefreshMessage() {
            _timer.Start();
        }
    }
}
