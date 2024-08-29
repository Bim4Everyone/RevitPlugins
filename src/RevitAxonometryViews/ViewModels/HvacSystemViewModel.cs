using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitAxonometryViews.ViewModels {
    internal class HvacSystemViewModel : BaseViewModel {
        private bool _isSelected;

        public HvacSystemViewModel(string systemName, string fopName) {
            SystemName = systemName;
            FopName = fopName;
        }

        public string SystemName { get; set; }
        public string FopName { get; set; }

        public bool IsSelected {
            get => _isSelected;
            set => RaiseAndSetIfChanged(ref _isSelected, value);
        }
    }
}