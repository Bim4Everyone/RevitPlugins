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
using RevitLintelPlacement.ViewModels.Interfaces;
using RevitLintelPlacement.ViewModels.LintelParameterViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class GroupedRuleViewModel : BaseViewModel {
        private string _name;
        private ObservableCollection<ConcreteRuleViewModel> _gruopedRules;
        private WallTypesConditionViewModel _groupingCondition;

        public GroupedRuleViewModel() {
            AddRuleCommand = new RelayCommand(AddRule, p => true);
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

        private void AddRule(object p) {
            Rules.Add(new ConcreteRuleViewModel());
        }

    }

    internal class ConcreteRuleViewModel : BaseViewModel {
        private string _name;
        private LintelTypeNameParameter _selectedLintelType;
        private OpeningWidthConditionViewModel _openingWidthCondition;
        private NumberLintelParameterViewModel _offsetParameterViewModel;
        private LintelLeftOffsetParameter _lintelLeftOffsetParameter;
        private LintelRightOffsetParameter _lintelRightOffsetParameter;
        private WallTypesConditionViewModel _wallTypesConditions;
        private List<LintelTypeNameParameter> _lintelTypes;
        private List<IConditionViewModel> _conditions;
        private List<ILintelParameterViewModel> _parameters;

        public ConcreteRuleViewModel() {

        }

        public ConcreteRuleViewModel(RuleSetting ruleSetting) {
            InitializeRule(ruleSetting);
            AddParameters();
            AddConditions();
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        public LintelTypeNameParameter SelectedLintelType {
            get => _selectedLintelType;
            set => this.RaiseAndSetIfChanged(ref _selectedLintelType, value);
        }

        public OpeningWidthConditionViewModel OpeningWidthCondition {
            get => _openingWidthCondition;
            set => this.RaiseAndSetIfChanged(ref _openingWidthCondition, value);
        }

        public NumberLintelParameterViewModel OffsetParameterViewModel {
            get => _offsetParameterViewModel;
            set => this.RaiseAndSetIfChanged(ref _offsetParameterViewModel, value);
        }

        public LintelLeftOffsetParameter LintelLeftOffsetParameter { 
            get => _lintelLeftOffsetParameter; 
            set => this.RaiseAndSetIfChanged(ref _lintelLeftOffsetParameter, value); 
        }

        public LintelRightOffsetParameter LintelRightOffsetParameter {
            get => _lintelRightOffsetParameter;
            set => this.RaiseAndSetIfChanged(ref _lintelRightOffsetParameter, value);
        }

        public WallHalfThicknessParameter WallHalfThicknessParameter { get; set; }

        public OpeningWidthParameter OpeningWidthParameter { get; set; }

        public CornerParamerer CornerParamerer { get; set; }

        public List<LintelTypeNameParameter> LintelTypes {
            get => _lintelTypes;
            set => this.RaiseAndSetIfChanged(ref _lintelTypes, value);
        }

        public WallTypesConditionViewModel WallTypesConditions {
            get => _wallTypesConditions;
            set => this.RaiseAndSetIfChanged(ref _wallTypesConditions, value);
        }

        private void InitializeRule(RuleSetting ruleSetting) {
            Name = ruleSetting.Name;

            foreach(var condition in ruleSetting.ConditionSettingsConfig) {
                switch(condition.ConditionType) {
                    case ConditionType.WallTypes: {
                        WallTypesConditions = new WallTypesConditionViewModel() {
                            WallTypes = new ObservableCollection<WallTypeConditionViewModel>(condition.WallTypes.Select(wt => new WallTypeConditionViewModel() {
                                Name = wt,
                                IsChecked = true
                            }))};
                        break;
                    }
                    case ConditionType.OpeningWidth: {
                        OpeningWidthCondition = new OpeningWidthConditionViewModel() {
                            MaxWidth = condition.OpeningWidthMax,
                            MinWidth = condition.OpeningWidthMin
                        };
                        break;
                    }
                }
            }

            foreach(var parameter in ruleSetting.LintelParameterSettingsConfig) {
                switch(parameter.LintelParameterType) {
                    case LintelParameterType.CornerParameter:
                    CornerParamerer = new CornerParamerer() {
                        IsCornerChecked = parameter.IsCornerChecked
                    };
                    break;
                    case LintelParameterType.LeftOffsetParameter: {
                        LintelLeftOffsetParameter = new LintelLeftOffsetParameter() {
                            LeftOffset = parameter.Offset
                        };
                        break;
                    }
                    case LintelParameterType.RightOffsetParameter: {
                        LintelRightOffsetParameter = new LintelRightOffsetParameter() {
                            RightOffset = parameter.Offset
                        };
                        break;
                    }
                    case LintelParameterType.TypeNameParameter: {
                        SelectedLintelType = new LintelTypeNameParameter() {
                            Name = parameter.LintelTypeName
                        };
                        break;
                    }
                }
            }

            OpeningWidthParameter = new OpeningWidthParameter();
            WallHalfThicknessParameter = new WallHalfThicknessParameter();
        }

        private void AddParameters() {
            _parameters = new List<ILintelParameterViewModel>();
            _parameters.Add(CornerParamerer);
            _parameters.Add(OpeningWidthParameter);
            _parameters.Add(WallHalfThicknessParameter);
            _parameters.Add(LintelRightOffsetParameter);
            _parameters.Add(LintelLeftOffsetParameter);
        }

        private void AddConditions() {
            _conditions.Add(OpeningWidthCondition);
            _conditions.Add(WallTypesConditions);
        }

    }
}
