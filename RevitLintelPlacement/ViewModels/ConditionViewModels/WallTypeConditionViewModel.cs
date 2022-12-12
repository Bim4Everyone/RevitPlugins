using System;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class WallTypeConditionViewModel : BaseViewModel, IEquatable<WallTypeConditionViewModel> {
        private string _name;
        private bool _isChecked;

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public bool IsChecked {
            get => _isChecked;
            set => _isChecked = value;
        }

        public override bool Equals(object obj) {
            return Equals(obj as WallTypeConditionViewModel);
        }

        public bool Equals(WallTypeConditionViewModel other) {
            return other != null
                && Name == other.Name;
        }

        public override int GetHashCode() {
            int hashCode = 897683154;
            hashCode = hashCode * -1521134295 + Name.GetHashCode();
            return hashCode;
        }
    }

}
