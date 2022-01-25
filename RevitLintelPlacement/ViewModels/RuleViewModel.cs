using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class RuleViewModel : BaseViewModel {
        private bool _isChecked;
        private string _name;
        private ConditionCollectionViewModel _conditions;
        private LintelParameterCollectionViewModel _lintelParameters;

        public RuleViewModel() {
            Conditions = new ConditionCollectionViewModel();
            LintelParameters = new LintelParameterCollectionViewModel();
        }

        public bool IsChecked {
            get => _isChecked;
            set => this.RaiseAndSetIfChanged(ref _isChecked, value);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public ConditionCollectionViewModel Conditions {
            get => _conditions;
            set => this.RaiseAndSetIfChanged(ref _conditions, value);
        }

        public LintelParameterCollectionViewModel LintelParameters {
            get => _lintelParameters;
            set => this.RaiseAndSetIfChanged(ref _lintelParameters, value);
        }
    }
}
