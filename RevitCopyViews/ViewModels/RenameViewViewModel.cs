using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitCopyViews.ViewModels {
    internal class RenameViewViewModel : BaseViewModel {
        private string _prefix;
        private string _suffix;
        private string _replaceOldText;
        private string _replaceNewText;

        private string _errorText;

        private bool _isAllowReplaceSuffix;
        private bool _isAllowReplacePrefix;

        public RenameViewViewModel() {
            RenameViewCommand = new RelayCommand(RenameView, CanRenameView);
        }

        public string Prefix {
            get => _prefix;
            set => this.RaiseAndSetIfChanged(ref _prefix, value);
        }

        public string Suffix {
            get => _suffix;
            set => this.RaiseAndSetIfChanged(ref _suffix, value);
        }

        public bool IsAllowReplacePrefix {
            get => _isAllowReplacePrefix;
            set => this.RaiseAndSetIfChanged(ref _isAllowReplacePrefix, value);
        }
        public bool IsAllowReplaceSuffix {
            get => _isAllowReplaceSuffix;
            set => this.RaiseAndSetIfChanged(ref _isAllowReplaceSuffix, value);
        }

        public string ReplaceOldText {
            get => _replaceOldText;
            set => this.RaiseAndSetIfChanged(ref _replaceOldText, value);
        }

        public string ReplaceNewText {
            get => _replaceNewText;
            set => this.RaiseAndSetIfChanged(ref _replaceNewText, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public ICommand RenameViewCommand { get; }

        private void RenameView(object p) {

        }

        private bool CanRenameView(object p) {
            ErrorText = null;
            return true;
        }
    }
}
