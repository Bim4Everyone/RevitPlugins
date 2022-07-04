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

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.ViewModels.Navigator {
    internal class ClashesViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private string _message;
        private string _selectedFile;
        private List<ClashViewModel> _clashes;
        private CollectionViewSource _clashesViewSource;
        private bool _openFromClashDetector;

        public ClashesViewModel(RevitRepository revitRepository, string selectedFile = null) {
            _revitRepository = revitRepository;

            if(selectedFile == null) {
                InitializeFiles();
            } else {
                InitializeFiles(selectedFile);
            }

            ClashesViewSource = new CollectionViewSource();
            InitializeClashesFromFile();

            SelectionChangedCommand = new RelayCommand(SelectionChanged);
            SelectClashCommand = new RelayCommand(SelectClash);
            SaveCommand = new RelayCommand(SaveConfig, CanSaveConfig);

            SelectionDataChangedCommand = new RelayCommand(SelectionDataChanged);
            OpenClashDetectorCommand = new RelayCommand(OpenClashDetector, p => OpenFromClashDetector);
        }

        public ICommand SelectClashCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SelectionChangedCommand { get; }
        public ICommand SelectionDataChangedCommand { get; }
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

        public CollectionViewSource ClashesViewSource {
            get => _clashesViewSource;
            set => this.RaiseAndSetIfChanged(ref _clashesViewSource, value);
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
                Clashes = config.Clashes.Select(item => new ClashViewModel(_revitRepository, item))
                    .Where(item => IsValid(documentNames, item))
                    .ToList();
                ClashesViewSource.Source = Clashes;
            }
        }

        private bool IsValid(List<string> documentNames, ClashViewModel clash) {
            var clashDocuments = new[] { clash.FirstDocumentName, clash.SecondDocumentName };
            return clashDocuments.All(item => documentNames.Any(d => d.Contains(item))) && clash.GetBoundingBox() != null;
        }

        private async void SelectClash(object p) {
            var clash = p as ClashViewModel;
            if(clash == null)
                return;
            await _revitRepository.SelectAndShowElement(clash.GetElementIds(_revitRepository.Doc.Title), clash.GetBoundingBox());
        }

        private async void SaveConfig(object p) {
            var config = ClashesConfig.GetClashesConfig(_revitRepository.GetObjectName(), SelectedFile);
            config.Clashes = Clashes.Select(item => GetUpdatedClash(item)).ToList();
            config.SaveProjectConfig();
            Message = "Файл успешно сохранен";
            await Task.Delay(3000);
            Message = null;
        }

        private bool CanSaveConfig(object p) {
            return SelectedFile != null;
        }

        private ClashModel GetUpdatedClash(ClashViewModel clash) {
            clash.Clash.IsSolved = clash.IsSolved;
            return clash.Clash;
        }

        private void SelectionChanged(object p) {
            InitializeClashesFromFile();
        }

        private void SelectionDataChanged(object p) {
            if(ClashesViewSource.View.CurrentPosition > -1
                && ClashesViewSource.View.CurrentPosition < Clashes.Count) {
                SelectClash(ClashesViewSource.View.CurrentItem);
            }
        }

        private void OpenClashDetector(object p) {
            _revitRepository.OpenClashDetectorWindow();
        }
    }
}
