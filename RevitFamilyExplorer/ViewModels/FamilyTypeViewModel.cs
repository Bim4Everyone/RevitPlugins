using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Autodesk.Revit.DB;

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

            RefreshImageSource();
        }

        public string Name { get; }
        public ICommand PlaceFamilySymbolCommand { get; }

        public string ImageSource {
            get => _imageSource;
            set => this.RaiseAndSetIfChanged(ref _imageSource, value);
        }

        public BitmapSource FamilySymbolIcon {
            get => _familySymbolIcon;
            set => this.RaiseAndSetIfChanged(ref _familySymbolIcon, value);
        }

        #region PlaceFamilySymbolCommand

        private void PlaceFamilySymbol(object p) {
            _revitRepository.PlaceFamilySymbol(_parentFamily.FileInfo, Name);
        }

        private bool CanPlaceFamilySymbol(object p) {
            return _revitRepository.CanPlaceFamilySymbol(_parentFamily.FileInfo, Name);
        }

        #endregion

        private void RefreshImageSource() {
#if D2020 || R2020
            ImageSource = CanPlaceFamilySymbol(null) 
                ? @"pack://application:,,,/RevitFamilyExplorer;component/Resources/place_family_symbol.png"
                : @"pack://application:,,,/RevitFamilyExplorer;component/Resources/cant_place_family_symbol.png";
#elif D2021 || R2021
                        ImageSource = CanPlaceFamilySymbol(null) 
                ? @"pack://application:,,,/RevitFamilyExplorer_2021;component/Resources/place_family_symbol.png"
                : @"pack://application:,,,/RevitFamilyExplorer_2021;component/Resources/cant_place_family_symbol.png";
#elif D2022 || R2022
                        ImageSource = CanPlaceFamilySymbol(null) 
                ? @"pack://application:,,,/RevitFamilyExplorer_2022;component/Resources/place_family_symbol.png"
                : @"pack://application:,,,/RevitFamilyExplorer_2022;component/Resources/cant_place_family_symbol.png";
#endif
        }
    }
}
