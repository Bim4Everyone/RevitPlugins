using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitFamilyExplorer.Models;

namespace RevitFamilyExplorer.ViewModels {
    internal class FamilyTypeViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly FamilyFileViewModel _parentFamily;

        private string _imageSource;
        private BitmapSource _familySymbolIcon;

        public FamilyTypeViewModel(RevitRepository revitRepository, FamilyFileViewModel parentFamily, string familyTypeName) {
            _revitRepository = revitRepository;
            _parentFamily = parentFamily;

            Name = familyTypeName;
            PlaceFamilySymbolCommand = new RelayCommand(PlaceFamilySymbol, CanPlaceFamilySymbol);
        }

        public string Name { get; }
        public ICommand PlaceFamilySymbolCommand { get; }
        public ICommand UpdateFamilyImageCommand { get; }

        public string ImageSource {
            get => _imageSource;
            set => this.RaiseAndSetIfChanged(ref _imageSource, value);
        }

        public BitmapSource FamilySymbolIcon {
            get => _familySymbolIcon;
            set => this.RaiseAndSetIfChanged(ref _familySymbolIcon, value);
        }

        #region PlaceFamilySymbolCommand

        private async void PlaceFamilySymbol(object p) {
            try {
                await _revitRepository.PlaceFamilySymbolAsync(_parentFamily.FileInfo, Name);
            } catch(OperationCanceledException) {
            } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
            }
        }

        private bool CanPlaceFamilySymbol(object p) {
            RefreshImageSource();
            return true;
        }

        #endregion

        private void RefreshImageSource() {
#if REVIT_2020
            ImageSource = _revitRepository.IsInsertedFamilySymbol(_parentFamily.FileInfo, Name)
                ? @"pack://application:,,,/RevitFamilyExplorer;component/Resources/insert.png"
                : @"pack://application:,,,/RevitFamilyExplorer;component/Resources/not-insert.png";
#else
            ImageSource = _revitRepository.IsInsertedFamilySymbol(_parentFamily.FileInfo, Name)
                ? $@"pack://application:,,,/RevitFamilyExplorer_{ModuleEnvironment.RevitVersion};component/Resources/insert.png" 
                : $@"pack://application:,,,/RevitFamilyExplorer_{ModuleEnvironment.RevitVersion};component/Resources/not-insert.png";
#endif
        }
    }
}
