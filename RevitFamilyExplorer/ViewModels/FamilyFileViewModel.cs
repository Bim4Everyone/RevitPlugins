using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.WindowsAPICodePack.Shell;

using RevitFamilyExplorer.Models;

namespace RevitFamilyExplorer.ViewModels {
    internal class FamilyFileViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private bool _isLoaded;
        private FileInfo _familyFile;
        private BitmapSource _image;



        public FamilyFileViewModel(RevitRepository revitRepository, FileInfo familyFile) {
            _revitRepository = revitRepository;
            _familyFile = familyFile;

            //Image = ShellFile.FromFilePath(_familyFile.FullName).Thumbnail.BitmapSource;

            ExpandCommand = new RelayCommand(Expand, CanExpand);
            FamilyTypes = new ObservableCollection<FamilyTypeViewModel>() { null };
        }

        public string Name {
            get { return _familyFile.Name; }
        }

        public BitmapSource Image {
            get => _image;
            set => this.RaiseAndSetIfChanged(ref _image, value);
        }

        public ICommand ExpandCommand { get; }
        public ObservableCollection<FamilyTypeViewModel> FamilyTypes { get; }

        public void Refresh(FileInfo newFileInfo) {
            _familyFile = newFileInfo;
            //Image = ShellFile.FromFilePath(_familyFile.FullName).Thumbnail.BitmapSource;

            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(Image));
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

        private IEnumerable<FamilyTypeViewModel> GetFamilySymbols() {
            return _revitRepository.GetFamilyTypes(_familyFile)
                .Select(item => new FamilyTypeViewModel(_revitRepository, item))
                .OrderBy(item => item.Name);
        }
    }
}