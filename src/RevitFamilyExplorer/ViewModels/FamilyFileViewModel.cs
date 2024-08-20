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

using dosymep.Bim4Everyone;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.WindowsAPICodePack.Shell;

using RevitFamilyExplorer.Models;

namespace RevitFamilyExplorer.ViewModels {
    internal class FamilyFileViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private bool _isLoaded;
        private string _imageSource;
        private FileInfo _familyFile;
        private BitmapSource _familyIcon;

        public FamilyFileViewModel(RevitRepository revitRepository, FileInfo familyFile) {
            _revitRepository = revitRepository;

            ExpandCommand = new RelayCommand(Expand, CanExpand);
            LoadFamilyCommand = new RelayCommand(LoadFamilyAsync, CanLoadFamily);

            FileInfo = familyFile;
            FamilyTypes = new ObservableCollection<FamilyTypeViewModel>() { null };
        }

        public FileInfo FileInfo {
            get { return _familyFile; }
            set {
                _familyFile = value;
                RaisePropertyChanged(nameof(Name));

                if(_familyFile != null) {
                    FamilyIcon = ShellFile.FromFilePath(_familyFile.FullName).Thumbnail.BitmapSource;
                }
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
            try {
                await _revitRepository.LoadFamilyAsync(_familyFile);
            } catch(OperationCanceledException) {
            } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
            }
        }

        private bool CanLoadFamily(object p) {
            bool isInsertedFamilyFile = _revitRepository.IsInsertedFamilyFile(_familyFile);
            RefreshImageSource(isInsertedFamilyFile);
            
            return !isInsertedFamilyFile;
        }

        #endregion

        private void RefreshImageSource(bool isInsertedFamilyFile) {
#if REVIT_2020
            ImageSource = isInsertedFamilyFile
                ? @"pack://application:,,,/RevitFamilyExplorer;component/Resources/insert.png"
                : @"pack://application:,,,/RevitFamilyExplorer;component/Resources/not-insert.png";
#else
            ImageSource = isInsertedFamilyFile 
                ? $@"pack://application:,,,/RevitFamilyExplorer_{ModuleEnvironment.RevitVersion};component/Resources/insert.png" 
                : $@"pack://application:,,,/RevitFamilyExplorer_{ModuleEnvironment.RevitVersion};component/Resources/not-insert.png";
#endif
        }

        private IEnumerable<FamilyTypeViewModel> GetFamilySymbols() {
            return _revitRepository.GetFamilyTypes(_familyFile)
                .Select(item => new FamilyTypeViewModel(_revitRepository, this, item))
                .OrderBy(item => item.Name);
        }
    }
}