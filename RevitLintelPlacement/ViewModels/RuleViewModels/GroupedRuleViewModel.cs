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
    internal class GroupedRuleViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly ElementInfosViewModel _elementInfos;
        private string _name;
        private ObservableCollection<ConcreteRuleViewModel> _groupedRules;
        private WallTypesConditionViewModel _groupingCondition;
        private string _errorText;

        public GroupedRuleViewModel(RevitRepository revitRepository, ElementInfosViewModel elementInfos, GroupedRuleSettings groupedRuleSettings = null) {
            this._revitRepository = revitRepository;
            this._elementInfos = elementInfos;
            AddRuleCommand = new RelayCommand(AddRule, p => true);
            RemoveRuleCommand = new RelayCommand(RemoveRule, p => true);
            if(groupedRuleSettings == null || groupedRuleSettings.Rules.Count == 0) {
                Rules = new ObservableCollection<ConcreteRuleViewModel>();
                var rule = new ConcreteRuleViewModel(revitRepository, _elementInfos);
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
                    .Select(r => new ConcreteRuleViewModel(_revitRepository, _elementInfos, r)));
            }
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string ErrorText { 
            get => _errorText; 
            set => this.RaiseAndSetIfChanged(ref _errorText, value); 
        }

        public WallTypesConditionViewModel WallTypes {
            get => _groupingCondition;
            set => this.RaiseAndSetIfChanged(ref _groupingCondition, value);
        }

        public ObservableCollection<ConcreteRuleViewModel> Rules {
            get => _groupedRules;
            set => this.RaiseAndSetIfChanged(ref _groupedRules, value);
        }

        public ICommand AddRuleCommand { get; set; }
        public ICommand RemoveRuleCommand { get; set; }

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
                Rules = Rules.Select(r => r.GetRuleSetting()).ToList()
            };
        }

        public ConcreteRuleViewModel GetRule(FamilyInstance familyInstance) {
            if(familyInstance is null) {
                throw new ArgumentNullException(nameof(familyInstance));
            }

            if(familyInstance.Host is Wall wall &&
                WallTypes.WallTypes.Any(e => e.IsChecked && e.Name.Equals(wall.Name, StringComparison.CurrentCultureIgnoreCase))) {
                foreach(var rule in Rules) {
                    if(rule.CheckConditions(familyInstance))
                        return rule;
                }
            }
            return null;
        }

        private void AddRule(object p) {
            Rules.Add(new ConcreteRuleViewModel(_revitRepository, _elementInfos));
        }
        private void RemoveRule(object p) {
            if(Rules.Count > 0) {
                Rules.Remove(Rules.Last());
            }
        }

        private void InitializeWallTypes() {
            WallTypes = new WallTypesConditionViewModel();
            WallTypes.WallTypes = new ObservableCollection<WallTypeConditionViewModel>(
                _revitRepository.GetWallTypes()
                .Select(w => new WallTypeConditionViewModel() {
                    Name = w.Name,
                    IsChecked = false
                }).OrderBy(e => e.Name));
        }

        public bool UpdateErrorText() {
            foreach(var rule in Rules) {
                if(rule.OpeningWidthCondition.MinWidth > rule.OpeningWidthCondition.MaxWidth) {
                    ErrorText = $"Минимальная ширина проема \"{rule.OpeningWidthCondition.MinWidth}\" " +
                        $"должна быть меньше максимальной ширины \"{rule.OpeningWidthCondition.MaxWidth}\"";
                    return false;
                }
                var index = Rules.IndexOf(rule);
                if (index + 1 < Rules.Count) {
                    for(int i = index + 1; i < Rules.Count; i++) {
                        if(!(rule.OpeningWidthCondition.MinWidth > Rules[i].OpeningWidthCondition.MaxWidth
                            || rule.OpeningWidthCondition.MaxWidth < Rules[i].OpeningWidthCondition.MinWidth)) {
                            ErrorText = $"Проем \"{Rules[i].OpeningWidthCondition.MinWidth} - {Rules[i].OpeningWidthCondition.MaxWidth}\" " +
                               $"пересекается с проемом \"{rule.OpeningWidthCondition.MinWidth} - {rule.OpeningWidthCondition.MaxWidth}\"";
                            return false;
                        }
                    }
                }
            }
            ErrorText = string.Empty;
            return true;
        }
    }
}
