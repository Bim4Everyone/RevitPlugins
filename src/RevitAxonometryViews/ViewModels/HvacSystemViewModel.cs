using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitAxonometryViews.ViewModels {
    internal class HvacSystemViewModel : BaseViewModel {
        private bool _isSelected;
        private string _displaySystemName;
        private string _displaySharedName;

        public HvacSystemViewModel(string systemName, string sharedName) {
            SystemName = systemName;
            SharedName = sharedName;

            _displaySystemName = systemName;
            _displaySharedName = sharedName;
        }

        public string SystemName { get; }
        public string SharedName { get; }

        public string DisplaySystemName {
            get => _displaySystemName;
            set => RaiseAndSetIfChanged(ref _displaySystemName, value);
        }

        public string DisplaySharedName {
            get => _displaySharedName;
            set => RaiseAndSetIfChanged(ref _displaySharedName, value);
        }

        public bool IsSelected {
            get => _isSelected;
            set => RaiseAndSetIfChanged(ref _isSelected, value);
        }
    }
}
