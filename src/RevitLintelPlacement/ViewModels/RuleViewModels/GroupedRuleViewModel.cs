using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels {
    internal class GroupedRuleViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly ElementInfosViewModel _elementInfos;
        private readonly GroupedRuleSettings _groupedRuleSettings;
        private string _name;
        private string _errorText;
        private string _filterText;
        private WallTypesConditionViewModel _wallTypes;
        private CollectionViewSource _wallTypesViewSource;
        private ObservableCollection<ConcreteRuleViewModel> _groupedRules;

        public GroupedRuleViewModel(RevitRepository revitRepository, ElementInfosViewModel elementInfos, GroupedRuleSettings groupedRuleSettings = null) {
            _revitRepository = revitRepository;
            _elementInfos = elementInfos;
            _groupedRuleSettings = groupedRuleSettings;
            AddRuleCommand = new RelayCommand(AddRule, p => true);
            RemoveRuleCommand = new RelayCommand(RemoveRule, p => true);
            if(_groupedRuleSettings == null || _groupedRuleSettings.Rules.Count == 0) {
                Rules = new ObservableCollection<ConcreteRuleViewModel>();
                var rule = new ConcreteRuleViewModel(revitRepository, _elementInfos);
                Rules.Add(rule);
                InitializeWallTypes();
            } else {
                InitializeWallTypes(_groupedRuleSettings);

                Name = _groupedRuleSettings.Name;
                Rules = new ObservableCollection<ConcreteRuleViewModel>(
                    _groupedRuleSettings.Rules
                    .Select(r => new ConcreteRuleViewModel(_revitRepository, _elementInfos, r)));
            }
            WallTypesViewSource = new CollectionViewSource() { Source = WallTypes.WallTypes };
            WallTypesViewSource.Filter += WallTypesViewSourceFilter;
            FilterWallsCommand = new RelayCommand(FilterWalls);
        }

        public string FilterText {
            get => _filterText;
            set => this.RaiseAndSetIfChanged(ref _filterText, value);
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
            get => _wallTypes;
            set => this.RaiseAndSetIfChanged(ref _wallTypes, value);
        }

        public CollectionViewSource WallTypesViewSource {
            get => _wallTypesViewSource;
            set => this.RaiseAndSetIfChanged(ref _wallTypesViewSource, value);
        }

        public ObservableCollection<ConcreteRuleViewModel> Rules {
            get => _groupedRules;
            set => this.RaiseAndSetIfChanged(ref _groupedRules, value);
        }


        public ICommand AddRuleCommand { get; set; }
        public ICommand RemoveRuleCommand { get; set; }
        public ICommand FilterWallsCommand { get; set; }

        public GroupedRuleSettings GetGroupedRuleSetting() {
            return new GroupedRuleSettings() {
                Name = Name,
                WallTypes = new ConditionSetting() {
                    ConditionType = ConditionType.WallTypes,
                    WallTypes = WallTypes.WallTypes
                    .Where(item => item.IsChecked)
                    .Select(item => item.Name)
                    .Union(GetOldNotShownWalls())
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

        public bool UpdateErrorText() {
            foreach(var rule in Rules) {
                if(rule.OpeningWidthCondition.MinWidth > rule.OpeningWidthCondition.MaxWidth) {
                    ErrorText = $"Минимальная ширина проема \"{rule.OpeningWidthCondition.MinWidth}\" " +
                        $"должна быть меньше максимальной ширины \"{rule.OpeningWidthCondition.MaxWidth}\"";
                    return false;
                }
                var index = Rules.IndexOf(rule);
                if(index + 1 < Rules.Count) {
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

        private void WallTypesViewSourceFilter(object sender, FilterEventArgs e) {
            if(e.Item is WallTypeConditionViewModel wallType) {
                e.Accepted = string.IsNullOrWhiteSpace(FilterText) || wallType.Name.IndexOf(FilterText, StringComparison.CurrentCultureIgnoreCase) >= 0;
            }
        }

        private void AddRule(object p) {
            Rules.Add(new ConcreteRuleViewModel(_revitRepository, _elementInfos));
        }
        private void RemoveRule(object p) {
            if(Rules.Count > 0) {
                Rules.Remove(Rules.Last());
            }
        }

        private void InitializeWallTypes(GroupedRuleSettings ruleSettings = null) {
            WallTypes = new WallTypesConditionViewModel();
            WallTypes.WallTypes = new ObservableCollection<WallTypeConditionViewModel>(
                _revitRepository.GetWallTypes()
                .Select(w => new WallTypeConditionViewModel() {
                    Name = w.Name,
                    IsChecked = ruleSettings?.WallTypes?.WallTypes?.Any(item => item.Equals(w.Name, StringComparison.CurrentCulture)) == true
                }).OrderBy(e => e.Name));
        }

        private void FilterWalls(object p) {
            WallTypesViewSource.View.Refresh();
        }

        private IEnumerable<string> GetOldNotShownWalls() {
            if(_groupedRuleSettings == null) {
                return Enumerable.Empty<string>();
            }
            return _groupedRuleSettings.WallTypes.WallTypes.Except(WallTypes.WallTypes.Select(item => item.Name));
        }
    }
}
