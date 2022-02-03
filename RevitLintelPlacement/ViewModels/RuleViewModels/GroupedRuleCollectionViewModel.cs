using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels {
    internal class GroupedRuleCollectionViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private ObservableCollection<GroupedRuleViewModel> _groupedRules;

        public GroupedRuleCollectionViewModel() {

        }

        public GroupedRuleCollectionViewModel(RevitRepository revitRepository, RulesSettigs rulesSettings = null) {
            AddGroupedRuleCommand = new RelayCommand(AddGroupedRule, p => true);
            RevoveGroupedRuleCommand = new RelayCommand(RemoveGroupedRule, p => true);
            SaveConfigCommand = new RelayCommand(SaveConfig, p => true);
            this._revitRepository = revitRepository;
            if (rulesSettings== null || rulesSettings.RuleSettings.Count == 0) {
                GroupedRules = new ObservableCollection<GroupedRuleViewModel>();
                var rule = new GroupedRuleViewModel(_revitRepository);
                GroupedRules.Add(rule);
            } else {
                GroupedRules = new ObservableCollection<GroupedRuleViewModel>(
                rulesSettings.RuleSettings.Select(r => new GroupedRuleViewModel(revitRepository, r))
                );
            }
        }

        public ICommand AddGroupedRuleCommand { get; set; }
        public ICommand RevoveGroupedRuleCommand { get; set; }
        public ICommand SaveConfigCommand { get; set; }

        public ObservableCollection<GroupedRuleViewModel> GroupedRules {
            get => _groupedRules; 
            set => this.RaiseAndSetIfChanged(ref _groupedRules, value);
        }

        public ConcreteRuleViewModel GetRule(FamilyInstance familyInstance) {
            if(familyInstance is null) {
                throw new ArgumentNullException(nameof(familyInstance));
            }

            foreach(var groupedRule in GroupedRules) {
                var rule = groupedRule.GetRule(familyInstance);
                if(rule != null) {
                    return rule;
                }
            }

            return null;
        }

        private void AddGroupedRule(object p) {
            GroupedRules.Add(new GroupedRuleViewModel(_revitRepository));
        }

        private void RemoveGroupedRule(object p) {
            if(p is GroupedRuleViewModel rule) {
                GroupedRules.Remove(rule);
            }
        }

        private void SaveConfig(object p) {
            var config = RuleConfig.GetRuleConfig();
            var settings = config.GetSettings(_revitRepository.GetDocumentName());
            if(settings == null) {
                settings = config.AddSettings(_revitRepository.GetDocumentName());
            }
            settings.RuleSettings = new List<GroupedRuleSettings>();
            foreach(var rule in GroupedRules) {
                settings.RuleSettings.Add(rule.GetGroupedRuleSetting());
            }

            config.SaveProjectConfig();
        }
    }
}
