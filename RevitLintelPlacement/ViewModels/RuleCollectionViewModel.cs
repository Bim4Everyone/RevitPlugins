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

        public RuleCollectionViewModel(RuleConfig ruleConfig) {
            Rules = new ObservableCollection<RuleViewModel>(MapRuleSettingsToRuleViewModels(ruleConfig.RuleSettingsConfig));
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
        private IEnumerable<RuleViewModel> MapRuleSettingsToRuleViewModels(List<RuleSettingsConfig> ruleSettings) {
            var ruleViewModels = new List<RuleViewModel>();
            foreach(var rs in ruleSettings) {
                var ruleViewModel = new RuleViewModel() {
                    Name = rs.Name,
                    Conditions = new ConditionCollectionViewModel(rs.ConditionSettingsConfig),
                    IsChecked = true
                };


                ruleViewModels.Add(ruleViewModel);
            }
            return ruleViewModels;
        }

        
    }
}
