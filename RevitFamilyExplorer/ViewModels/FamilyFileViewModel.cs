using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.WindowsAPICodePack.Shell;

using RevitFamilyExplorer.Models;

namespace RevitFamilyExplorer.ViewModels {
    internal class FamilyFileViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly Dispatcher _dispatcher;

        private bool _isLoaded;
        private string _imageSource;
        private FileInfo _familyFile;
        private BitmapSource _familyIcon; 

        public FamilyFileViewModel(RevitRepository revitRepository, FileInfo familyFile) {
            _revitRepository = revitRepository;
            _dispatcher = Dispatcher.CurrentDispatcher;
            
            ExpandCommand = new RelayCommand(Expand, CanExpand);
            LoadFamilyCommand = new RelayCommand(LoadFamilyAsync, CanLoadFamily);
            UpdateFamilyImageCommand = new RelayCommand(UpdateFamilyImage, CanUpdateFamilyImage);
            
            FileInfo = familyFile;
            FamilyTypes = new ObservableCollection<FamilyTypeViewModel>() { null };
        }

        public FileInfo FileInfo {
            get { return _familyFile; }
            set {
                _familyFile = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public string Name {
            get { return _familyFile.Name; }
        }

        public string ImageSource {
            get => _imageSource;
            set => this.RaiseAndSetIfChanged(ref _imageSource, value);
        }

        public BitmapSource FamilyIcon {
            get => _familyIcon;
            set => this.RaiseAndSetIfChanged(ref _familyIcon, value);
        }

        public ICommand ExpandCommand { get; }
        public ICommand LoadFamilyCommand { get; }
        public ICommand UpdateFamilyImageCommand { get; }
        public ObservableCollection<FamilyTypeViewModel> FamilyTypes { get; }

        #region ExpandCommand

        private void Expand(object p) {
            _isLoaded = true;
            FamilyTypes.Clear();

            foreach(var familyType in GetFamilySymbols()) {
                FamilyTypes.Add(familyType);
            }
        }

        private bool CanExpand(object p) {
            return !_isLoaded;
        }

        #endregion

        #region LoadFamilyCommand

        private async void LoadFamilyAsync(object p) {
            await _revitRepository.LoadFamilyAsync(_familyFile);
        }

        private bool CanLoadFamily(object p) {
            RefreshImageSource();
            return !_revitRepository.IsInsertedFamilyFile(_familyFile);
        }

        #endregion

        #region UpdateFamilyImageCommand

        private void UpdateFamilyImage(object p) {
            RefreshImageSource();
        }

        private bool CanUpdateFamilyImage(object p) {
            return true;
        }

        #endregion

        private void RefreshImageSource() {
#if D2020 || R2020
            ImageSource = _revitRepository.IsInsertedFamilyFile(_familyFile)
                ? @"pack://application:,,,/RevitFamilyExplorer;component/Resources/insert.png"
                : @"pack://application:,,,/RevitFamilyExplorer;component/Resources/not-insert.png";
#elif D2021 || R2021
            ImageSource = _revitRepository.IsInsertedFamilyFile(newFileInfo) 
                ? @"pack://application:,,,/RevitFamilyExplorer_2021;component/Resources/insert.png" 
                : @"pack://application:,,,/RevitFamilyExplorer_2021;component/Resources/not-insert.png";
#elif D2022 || R2022
            ImageSource = _revitRepository.IsInsertedFamilyFile(newFileInfo) 
                ? @"pack://application:,,,/RevitFamilyExplorer_2022;component/Resources/insert.png" 
                : @"pack://application:,,,/RevitFamilyExplorer_2022;component/Resources/not-insert.png";
#endif
            
            FamilyIcon = ShellFile.FromFilePath(_familyFile.FullName).Thumbnail.BitmapSource;
        }

        private IEnumerable<FamilyTypeViewModel> GetFamilySymbols() {
            return _revitRepository.GetFamilyTypes(_familyFile)
                .Select(item => new FamilyTypeViewModel(_revitRepository, this, item))
                .OrderBy(item => item.Name);
        }
    }
}