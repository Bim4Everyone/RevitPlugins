using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels {
    internal class GroupedRuleCollectionViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly IEnumerable<RulesSettigs> _rulesSettings;
        private ObservableCollection<GroupedRuleViewModel> _groupedRules;

        public GroupedRuleCollectionViewModel() {

        }
        public GroupedRuleCollectionViewModel(RevitRepository revitRepository, IEnumerable<RulesSettigs> rulesSettings) {
            AddGroupedRuleCommand = new RelayCommand(AddGroupedRule, p => true);
            this._revitRepository = revitRepository;
            this._rulesSettings = rulesSettings;
            GroupedRules = new ObservableCollection<GroupedRuleViewModel>();
            var concreateRuleViewModels = GetRuleViewModels(rulesSettings)
                .GroupBy(r => r.WallTypesConditions.WallTypes, new CollcectionComperer<WallTypeConditionViewModel>())
                .Select(g => new GroupedRuleViewModel() {
                    Name = g.First().Name,
                    WallTypes = g.First().WallTypesConditions,
                    Rules = new ObservableCollection<ConcreteRuleViewModel>(g),
                });
        }

        public ICommand AddGroupedRuleCommand { get; set; }

        public ObservableCollection<GroupedRuleViewModel> GroupedRules {
            get => _groupedRules; 
            set => this.RaiseAndSetIfChanged(ref _groupedRules, value);
        }

        private void AddGroupedRule(object p) {
            GroupedRules.Add(new GroupedRuleViewModel());
        }
        private IEnumerable<ConcreteRuleViewModel> GetRuleViewModels(IEnumerable<RulesSettigs> ruleSettings) {

            foreach(var rules in ruleSettings) {
                foreach(var rule in rules.RuleSettings) {
                    yield return new ConcreteRuleViewModel(rule);
                }
            }
        }
    }

    internal class CollcectionComperer<T> : IEqualityComparer<ObservableCollection<T>> {
        
        public bool Equals(ObservableCollection<T> x, ObservableCollection<T> y) {
            return x.SequenceEqual(y);
        }

        public int GetHashCode(ObservableCollection<T> obj) {
            return obj?.Count ?? 0;
        }
    }
}
