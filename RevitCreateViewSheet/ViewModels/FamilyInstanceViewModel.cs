
using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitCreateViewSheet.ViewModels {
    internal class FamilyInstanceViewModel : BaseViewModel {
        private readonly FamilyInstance _familyInstance;

        public FamilyInstanceViewModel(FamilyInstance familyInstance) {
            _familyInstance = familyInstance;
        }

        public string Name {
            get => _familyInstance.Name;
        }
    }
}
