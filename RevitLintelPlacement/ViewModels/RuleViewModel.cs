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
        private ConditionCollectionViewModel _conditionCollection;

        public RuleViewModel() {
            ConditionCollection = new ConditionCollectionViewModel();
        }

        public bool IsChecked {
            get => _isChecked;
            set => this.RaiseAndSetIfChanged(ref _isChecked, value);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public ConditionCollectionViewModel ConditionCollection { 
            get => _conditionCollection; 
            set => this.RaiseAndSetIfChanged(ref _conditionCollection, value); 
        }
    }
}
