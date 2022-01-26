using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class RuleCollectionViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly IEnumerable<RulesSettigs> _rulesSettings;
        private RuleViewModel _selectedRule;
        private ObservableCollection<RuleViewModel> _rules;

        public RuleCollectionViewModel() {

        }

        public RuleCollectionViewModel(RevitRepository revitRepository, IEnumerable<RulesSettigs> rulesSettings) {
            this._revitRepository = revitRepository;
            this._rulesSettings = rulesSettings;
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

        public bool CheckConditions(FamilyInstance elementInWall) {
            return Rules.Select(r => r.Conditions).All(c => c.CheckConditions(elementInWall));
        }

        //TODO: возможно, правила из шаблона нужно сделать нередактируемыми
        private IEnumerable<RuleViewModel> GetRuleViewModels(IEnumerable<RulesSettigs> ruleSettings) {
            
            foreach(var rules in ruleSettings) {
                foreach(var rule in rules.RuleSettings) {
                    yield return new RuleViewModel() {
                        Name = rule.Name,
                        IsSystem = rules.IsSystem,
                        Conditions = new ConditionCollectionViewModel(_revitRepository, rule.ConditionSettingsConfig),
                        LintelParameters = new LintelParameterCollectionViewModel(rule.LintelParameterSettingsConfig),
                        IsChecked = true
                    };
                }
            }
        }
    }
}
