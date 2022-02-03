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
    internal class GroupedRuleViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private string _name;
        private ObservableCollection<ConcreteRuleViewModel> _gruopedRules;
        private WallTypesConditionViewModel _groupingCondition;

        public GroupedRuleViewModel(RevitRepository revitRepository, GroupedRuleSettings groupedRuleSettings=null) {
            this._revitRepository = revitRepository;
            AddRuleCommand = new RelayCommand(AddRule, p => true);
            if (groupedRuleSettings==null || groupedRuleSettings.Rules.Count == 0) {
                Rules = new ObservableCollection<ConcreteRuleViewModel>();
                var rule = new ConcreteRuleViewModel(revitRepository);
                Rules.Add(rule);
                InitializeWallTypes();
            } else {
                InitializeWallTypes();
                foreach(var wallType in WallTypes.WallTypes) {
                    if(groupedRuleSettings.WallTypes.WallTypes
                        .Any(e => e.Equals(wallType.Name, StringComparison.CurrentCultureIgnoreCase)))
                        wallType.IsChecked = true;
                }
                Name = groupedRuleSettings.Name;
                Rules = new ObservableCollection<ConcreteRuleViewModel>(
                    groupedRuleSettings.Rules
                    .Select(r => new ConcreteRuleViewModel(_revitRepository, r)
                    ));
            }

        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public WallTypesConditionViewModel WallTypes {
            get => _groupingCondition;
            set => this.RaiseAndSetIfChanged(ref _groupingCondition, value);
        }

        public ObservableCollection<ConcreteRuleViewModel> Rules {
            get => _gruopedRules;
            set => this.RaiseAndSetIfChanged(ref _gruopedRules, value);
        }

        public ICommand AddRuleCommand { get; set; }

        public GroupedRuleSettings GetGroupedRuleSetting() {
            return new GroupedRuleSettings() {
                Name = Name,
                WallTypes = new ConditionSetting() {
                    ConditionType = ConditionType.WallTypes,
                    WallTypes = WallTypes.WallTypes
                    .Where(e => e.IsChecked)
                    .Select(e => e.Name)
                    .ToList()
                },
                Rules = Rules.Select(r=>r.GetRuleSetting()).ToList()
            };
        }

        private void AddRule(object p) {
            Rules.Add(new ConcreteRuleViewModel(_revitRepository));
        }

        private void InitializeWallTypes() {
            WallTypes = new WallTypesConditionViewModel();
            WallTypes.WallTypes = new ObservableCollection<WallTypeConditionViewModel>(
                _revitRepository.GetWallTypes()
                .Select(w => new WallTypeConditionViewModel() {
                    Name = w.Name,
                    IsChecked = false
                }));
        }
    }
}
