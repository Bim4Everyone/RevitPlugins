using System.Windows.Input;

using dosymep.WPF.ViewModels;

namespace PlatformSettings.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private string _errorText;
        
        
        public ICommand LoadViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }
    }
}