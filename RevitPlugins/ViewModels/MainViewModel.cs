using Autodesk.Revit.UI;

using dosymep.WPF.ViewModels;

using RevitPlugins.Models;

namespace RevitPlugins.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private string _errorText;

        public MainViewModel(UIApplication uiApplication) {
            _revitRepository = new RevitRepository(uiApplication);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }
    }
}