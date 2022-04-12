using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.WPF.ViewModels;

namespace RevitWindowGapPlacement.ViewModels {
    internal class SettingsViewModel : BaseViewModel {
        private string _errorText;
        private string _windowTitle;
        
        public ICommand PerformWindowCommand { get; }
        public ObservableCollection<CommandViewModel> AdditionCommands { get; }
        
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string WindowTitle {
            get => _windowTitle;
            set => this.RaiseAndSetIfChanged(ref _windowTitle, value);
        }
    }
}