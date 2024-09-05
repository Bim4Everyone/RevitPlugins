using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitAxonometryViews.ViewModels {
    internal class HvacSystemViewModel : BaseViewModel {
        private bool _isSelected;

        public HvacSystemViewModel(string systemName, string sharedName) {
            SystemName = systemName;
            SharedName = sharedName;
        }

        public string SystemName { get; }
        public string SharedName { get; }

        public bool IsSelected {
            get => _isSelected;
            set => RaiseAndSetIfChanged(ref _isSelected, value);
        }
    }
}
