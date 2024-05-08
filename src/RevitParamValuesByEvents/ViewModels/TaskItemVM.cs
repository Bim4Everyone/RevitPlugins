using dosymep.WPF.ViewModels;

namespace RevitParamValuesByEvents.ViewModels {
    internal class TaskItemVM : BaseViewModel {
        private bool _isCheck;
        private string _paramName;
        private string _paramValue;

        public TaskItemVM(bool isCheck, string paramName, string paramValue) {

            IsCheck = isCheck;
            ParamName = paramName;
            ParamValue = paramValue;
        }


        public bool IsCheck {
            get => _isCheck;
            set => this.RaiseAndSetIfChanged(ref _isCheck, value);
        }

        public string ParamName {
            get => _paramName;
            set => this.RaiseAndSetIfChanged(ref _paramName, value);
        }

        public string ParamValue {
            get => _paramValue;
            set => this.RaiseAndSetIfChanged(ref _paramValue, value);
        }
    }
}
