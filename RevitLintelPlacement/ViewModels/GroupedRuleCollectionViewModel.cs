using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class GroupedRuleCollectionViewModel : BaseViewModel {
        private ObservableCollection<GroupedRuleViewModel> _groupedRules;

        public GroupedRuleCollectionViewModel() {
            AddGroupedRuleCommand = new RelayCommand(AddGroupedRule, p => true);
        }

        public ICommand AddGroupedRuleCommand { get; set; }

        public ObservableCollection<GroupedRuleViewModel> GroupedRules {
            get => _groupedRules; 
            set => this.RaiseAndSetIfChanged(ref _groupedRules, value);
        }

        private void AddGroupedRule(object p) {
            GroupedRules.Add(new GroupedRuleViewModel());
        }
    }
}
