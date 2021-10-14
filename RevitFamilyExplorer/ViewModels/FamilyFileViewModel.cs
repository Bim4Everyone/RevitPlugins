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
            
            FamilyTypes = new ObservableCollection<FamilyTypeViewModel>() { null };
            Refresh(familyFile);
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

        public bool IsInsertedFamilyFile() {
            return _revitRepository.IsInsertedFamilyFile(_familyFile);
        }

        public void Refresh(FileInfo newFileInfo) {
            _familyFile = newFileInfo;
            RaisePropertyChanged(nameof(Name));

            RefreshImageSource();
            FamilyIcon = ShellFile.FromFilePath(_familyFile.FullName).Thumbnail.BitmapSource;
        }

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

        private void LoadFamilyAsync(object p) {
            _revitRepository.LoadFamilyAsync(_familyFile)
                .ContinueWith(t => {
                    RefreshImageSource();

                    _dispatcher.Invoke(() => {
                        FamilyTypes.Clear();
                        var familyTypes = _revitRepository.GetFamilySymbols(_familyFile)
                            .Select(item => new FamilyTypeViewModel(_revitRepository, this, item.Name) { FamilySymbolIcon = _revitRepository.GetFamilySymbolIcon(item) })
                            .OrderBy(item => item.Name);

                        foreach(var familyType in familyTypes) {
                            FamilyTypes.Add(familyType);
                        }

                        _isLoaded = true;
                    });
                });
        }

        private bool CanLoadFamily(object p) {
            return !_revitRepository.IsInsertedFamilyFile(_familyFile);
        }

        private void UpdateFamilyImage(object p) {
            RefreshImageSource();
        }

        private bool CanUpdateFamilyImage(object p) {
            return true;
        }

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
        }

        private IEnumerable<FamilyTypeViewModel> GetFamilySymbols() {
            return _revitRepository.GetFamilyTypes(_familyFile)
                .Select(item => new FamilyTypeViewModel(_revitRepository, this, item))
                .OrderBy(item => item.Name);
        }
    }
}