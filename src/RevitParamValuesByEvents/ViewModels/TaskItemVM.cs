using System.Collections.Generic;
using System.Linq;

using dosymep.WPF.ViewModels;

namespace RevitParamValuesByEvents.ViewModels {
    internal class TaskItemVM : BaseViewModel {
        private bool _isCheck = true;
        private string _selectedParamName = string.Empty;
        private string _paramValue = string.Empty;
        private List<string> _paramNames;

        public TaskItemVM(List<string> paramNames) {

            ParamNames = paramNames;
            SelectedParamName = paramNames.FirstOrDefault();
        }


        public bool IsCheck {
            get => _isCheck;
            set => this.RaiseAndSetIfChanged(ref _isCheck, value);
        }

        public string SelectedParamName {
            get => _selectedParamName;
            set => this.RaiseAndSetIfChanged(ref _selectedParamName, value);
        }

        public List<string> ParamNames {
            get => _paramNames;
            set => this.RaiseAndSetIfChanged(ref _paramNames, value);
        }


        public string ParamValue {
            get => _paramValue;
            set => this.RaiseAndSetIfChanged(ref _paramValue, value);
        }
    }
}
