using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class RuleCollectionViewModel : BaseViewModel {

        private RuleViewModel _selectedRule;
        private ObservableCollection<RuleViewModel> _rules;

        public RuleCollectionViewModel() {

        }

        public RuleCollectionViewModel(IEnumerable<RulesSettigs> rulesSettings) {
            Rules = new ObservableCollection<RuleViewModel>(GetRuleViewModels(rulesSettings));
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

        //TODO: возможно, правила из шаблона нужно сделать нередактируемыми
        private IEnumerable<RuleViewModel> GetRuleViewModels(IEnumerable<RulesSettigs> ruleSettings) {
            
            foreach(var rules in ruleSettings) {
                foreach(var rule in rules.RuleSettings) {
                    yield return new RuleViewModel() {
                        Name = rule.Name,
                        IsSystem = rules.IsSystem,
                        Conditions = new ConditionCollectionViewModel(rule.ConditionSettingsConfig),
                        IsChecked = true
                    };
                }
            }
        }
    }
}
