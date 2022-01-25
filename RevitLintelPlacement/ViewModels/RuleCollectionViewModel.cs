using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class RuleCollectionViewModel : BaseViewModel {

        private RuleViewModel _selectedRule;
        private ObservableCollection<RuleViewModel> _rules;

        public RuleCollectionViewModel() {
            Rules = new ObservableCollection<RuleViewModel>();
            SelectedRule = Rules.FirstOrDefault();
        }

        public RuleViewModel SelectedRule { 
            get => _selectedRule; 
            set => this.RaiseAndSetIfChanged(ref _selectedRule, value); 
        }

        public ObservableCollection<RuleViewModel> Rules {
            get => _rules;
            set => this.RaiseAndSetIfChanged(ref _rules, value);
        }
    }
}
