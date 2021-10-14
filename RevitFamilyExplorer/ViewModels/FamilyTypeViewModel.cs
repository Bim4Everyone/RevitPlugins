using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitFamilyExplorer.Models;

namespace RevitFamilyExplorer.ViewModels {
    internal class FamilyTypeViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly FamilyFileViewModel _parentFamily;

        public FamilyTypeViewModel(RevitRepository revitRepository, FamilyFileViewModel parentFamily, string familyTypeName) {
            _revitRepository = revitRepository;
            _parentFamily = parentFamily;
           
            Name = familyTypeName;
            PlaceFamilySymbolCommand = new RelayCommand(PlaceFamilySymbol, CanPlaceFamilySymbol);
        }

        public string Name { get; }
        public ICommand PlaceFamilySymbolCommand { get; }

        private void PlaceFamilySymbol(object p) {
            _revitRepository.PlaceFamilySymbol(Name);
        }

        private bool CanPlaceFamilySymbol(object p) {
            return _parentFamily.IsInsertedFamilyFile();
        }
    }
}
