using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitCopyViews.ViewModels {
    internal class CopyUserViewModel : BaseViewModel {
        private string _prefix;
        private string _lastName;
        private string _errorText;

        public CopyUserViewModel() {
            CopyUserCommand = new RelayCommand(CopyUser, CanCopyUser);
        }

        public List<string> GroupViews { get; set; }
        public List<string> RestrictedViewNames { get; set; }

        public string Prefix {
            get => _prefix;
            set => this.RaiseAndSetIfChanged(ref _prefix, value);
        }

        public string LastName {
            get => _lastName;
            set => this.RaiseAndSetIfChanged(ref _lastName, value);
        }

        public string GroupView {
            get { return "01_" + LastName; } 
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public ICommand CopyUserCommand { get; }

        private void CopyUser(object p) {

        }

        private bool CanCopyUser(object p) {
            if(GroupViews.Any(item => item.Equals(GroupView, StringComparison.CurrentCultureIgnoreCase))) {
                ErrorText = "Данная группа видов уже используется.";
                return false;
            }

            if(RestrictedViewNames.Any(item => item.StartsWith(Prefix, StringComparison.CurrentCultureIgnoreCase))) {
                ErrorText = "Данный префикс уже используется.";
                return false;
            }

            return true;
        }
    }
}
