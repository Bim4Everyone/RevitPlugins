using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class RuleCollectionViewModel : BaseViewModel {

        private ObservableCollection<RuleViewModel> _ruleViewModelCollection;
        private RuleViewModel _selectedRuleViewModel;

        public ObservableCollection<RuleViewModel> RuleViewModelCollection {
            get => _ruleViewModelCollection;
            set => this.RaiseAndSetIfChanged(ref _ruleViewModelCollection, value);
        }

        public RuleViewModel SelectedRuleViewModel { 
            get => _selectedRuleViewModel; 
            set => this.RaiseAndSetIfChanged(ref _selectedRuleViewModel, value); 
        }
    }
}
