using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitAxonometryViews.Models {
    internal class HvacSystem : BaseViewModel {
        public string SystemName { get; set; }
        public string FopName { get; set; }
        public Element SystemElement { get; set; }

        private bool _isChecked;


        public bool IsChecked {
            get => _isChecked;
            set => RaiseAndSetIfChanged(ref _isChecked, value);

        }
    }
}
