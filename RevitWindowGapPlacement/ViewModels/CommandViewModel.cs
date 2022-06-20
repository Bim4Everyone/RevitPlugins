using System.Windows.Input;

using dosymep.WPF.ViewModels;

namespace RevitWindowGapPlacement.ViewModels
{
    internal class CommandViewModel : BaseViewModel {
        private string _toolTip;
        private ICommand _command;
        private string _imageSource;

        public string ToolTip {
            get => _toolTip;
            set => this.RaiseAndSetIfChanged(ref _toolTip, value);
        }

        public ICommand Command {
            get => _command;
            set => this.RaiseAndSetIfChanged(ref _command, value);
        }

        public string ImageSource {
            get => _imageSource;
            set => this.RaiseAndSetIfChanged(ref _imageSource, value);
        }
    }
}