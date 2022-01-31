using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class GroupedRuleCollectionViewModel : BaseViewModel {
        private ObservableCollection<GroupedRuleViewModel> _groupedRules;

        public ObservableCollection<GroupedRuleViewModel> GroupedRules {
            get => _groupedRules; 
            set => this.RaiseAndSetIfChanged(ref _groupedRules, value);
        }
    }
}
