
using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitCreateViewSheet.ViewModels {
    internal class TitleBlockViewModel : BaseViewModel {
        private readonly FamilySymbol _familySymbol;

        public TitleBlockViewModel(FamilySymbol familySymbol) {
            _familySymbol = familySymbol;
        }

        public FamilySymbol FamilySymbol {
            get => _familySymbol;
        }

        public string Name {
            get => $"{_familySymbol.FamilyName}: {_familySymbol.Name}";
        }

        public string FamilyName {
            get => _familySymbol.Name;
        }

        public override string ToString() {
            return Name;
        }
    }
}
