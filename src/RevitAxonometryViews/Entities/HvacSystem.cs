using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitAxonometryViews.Models {
    internal class HvacSystem : BaseViewModel {

        private bool _isSelected;

        public string SystemName { get; set; }
        public string FopName { get; set; }
        public Element SystemElement { get; set; }

        public bool IsSelected {
            get => _isSelected;
            set => RaiseAndSetIfChanged(ref _isSelected, value);
        }
    }
}
