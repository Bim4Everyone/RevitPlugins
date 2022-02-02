
using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class WallTypeConditionViewModel : BaseViewModel {
        private bool _isChecked;
        private string _name;

        public bool IsChecked {
            get => _isChecked;
            set => this.RaiseAndSetIfChanged(ref _isChecked, value);
        }

        public string Name { 
            get => _name; 
            set => this.RaiseAndSetIfChanged(ref _name, value); 
        }

        public bool Check(FamilyInstance elementInWall) {
            throw new System.NotImplementedException();
        }
    }

}
