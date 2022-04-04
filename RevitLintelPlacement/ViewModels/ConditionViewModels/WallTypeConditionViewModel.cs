using System;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class WallTypeConditionViewModel : BaseViewModel, IEquatable<WallTypeConditionViewModel> {
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

        public bool Equals(WallTypeConditionViewModel other) {
            return other != null &&
                   Name == other.Name &&
                   IsChecked == other.IsChecked;
        }

        public override int GetHashCode() {
            int hashCode = 897683154;
            hashCode = hashCode * -1521134295 + Name.GetHashCode();
            hashCode = hashCode * -1521134295 + IsChecked.GetHashCode();
            return hashCode;
        }
    }

}
