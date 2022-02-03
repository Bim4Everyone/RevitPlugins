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
        private readonly RulesSettigs _rulesSettings;
        private RuleViewModel _selectedRule;
        private ObservableCollection<RuleViewModel> _rules;

        public RuleCollectionViewModel() {

        }

        public RuleCollectionViewModel(RevitRepository revitRepository, RulesSettigs rulesSettings) {
            this._revitRepository = revitRepository;
            this._rulesSettings = rulesSettings;
            Rules = new ObservableCollection<RuleViewModel>(GetRuleViewModels(_rulesSettings));
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

        public RuleViewModel GetRule(FamilyInstance elementInWall) {
            return Rules.FirstOrDefault(r => r.Conditions.Conditions.All(c => c.Check(elementInWall)));
        }

        private IEnumerable<RuleViewModel> GetRuleViewModels(RulesSettigs ruleSettings) {
            foreach(var rule in ruleSettings.RuleSettings) {
                yield return new RuleViewModel() {
                    Name = rule.Name,
                    //Conditions = new ConditionCollectionViewModel(_revitRepository, rule.ConditionSettingsConfig),
                    //LintelParameters = new LintelParameterCollectionViewModel(rule.LintelParameterSettingsConfig),
                    //IsChecked = true
                };
            }
        }
    }
}
