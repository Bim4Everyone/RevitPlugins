using Autodesk.Revit.UI;

using dosymep.WPF.ViewModels;

using RevitCopingZones.Models;

namespace RevitCopingZones.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly CopingZonesConfig _copingZonesConfig;

        private string _errorText;

        public MainViewModel(RevitRepository revitRepository, CopingZonesConfig copingZonesConfig) {
            _revitRepository = revitRepository;
            _copingZonesConfig = copingZonesConfig;
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }
    }
}