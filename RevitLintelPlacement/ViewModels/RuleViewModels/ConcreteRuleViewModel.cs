using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels.Interfaces;
using RevitLintelPlacement.ViewModels.LintelParameterViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class ConcreteRuleViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private string _selectedLintelType;
        private OpeningWidthConditionViewModel _openingWidthCondition;
        private LintelLeftOffsetParameter _lintelLeftOffsetParameter;
        private LintelRightOffsetParameter _lintelRightOffsetParameter;
        private List<string> _lintelTypes;
        private List<IConditionViewModel> _conditions = new List<IConditionViewModel>();
        private List<ILintelParameterViewModel> _parameters = new List<ILintelParameterViewModel>();

        public ConcreteRuleViewModel() {
        }

        public ConcreteRuleViewModel(RevitRepository revitRepository, RuleSetting ruleSetting = null) {
            this._revitRepository = revitRepository;
            if(ruleSetting == null) {
                InitializeEmptyGroupedRule();
            } else {
                InitializeGroupedRule(ruleSetting);
            }
            AddParameters();
            AddConditions();
        }

        public string SelectedLintelType {
            get => _selectedLintelType;
            set => this.RaiseAndSetIfChanged(ref _selectedLintelType, value);
        }

        public OpeningWidthConditionViewModel OpeningWidthCondition {
            get => _openingWidthCondition;
            set => this.RaiseAndSetIfChanged(ref _openingWidthCondition, value);
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

        public LintelCornerParamerer CornerParamerer { get; set; }

        public List<string> LintelTypes {
            get => _lintelTypes;
            set => this.RaiseAndSetIfChanged(ref _lintelTypes, value);
        }

        public bool CheckConditions(FamilyInstance familyInstance) {
            return _conditions.All(c => c.Check(familyInstance));
        }

        public RuleSetting GetRuleSetting() {
            return new RuleSetting() {
                LintelTypeName=SelectedLintelType,
                ConditionSettingsConfig = GetConditionSettings().ToList(),
                LintelParameterSettingsConfig = GetParameterSettings().ToList()
            };
        }

        public void SetParametersTo(FamilyInstance lintel, FamilyInstance elementInWall) {
            foreach(var parameter in _parameters) {
                parameter.SetTo(lintel, elementInWall);
            }
        }

        private IEnumerable<ConditionSetting> GetConditionSettings() {
            foreach(var condition in _conditions) {
                if (condition is OpeningWidthConditionViewModel openingCondition) {
                    yield return new ConditionSetting() {
                        OpeningWidthMax = openingCondition.MaxWidth,
                        OpeningWidthMin = openingCondition.MinWidth,
                        ConditionType = ConditionType.OpeningWidth
                    };
                }
            }
        }

        private IEnumerable<LintelParameterSetting> GetParameterSettings() {
            foreach(var parameter in _parameters) {
                if (parameter is LintelLeftOffsetParameter leftOffsetparameter) {
                    yield return new LintelParameterSetting() {
                        LintelParameterType = LintelParameterType.LeftOffsetParameter,
                        Offset = leftOffsetparameter.LeftOffset
                    };
                    yield return new LintelParameterSetting() {
                        LintelParameterType = LintelParameterType.RightOffsetParameter,
                        Offset = leftOffsetparameter.LeftOffset
                    };
                }
                if (parameter is LintelCornerParamerer cornerParameter) {
                    yield return new LintelParameterSetting() {
                        LintelParameterType = LintelParameterType.CornerParameter,
                        IsCornerChecked = cornerParameter.IsCornerChecked
                    };
                }
            }
        }

        private void InitializeGroupedRule(RuleSetting ruleSetting) {

            foreach(var condition in ruleSetting.ConditionSettingsConfig) {
                switch(condition.ConditionType) {
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
                    CornerParamerer = new LintelCornerParamerer() {
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
                }
            }
            LintelRightOffsetParameter.RightOffset = LintelLeftOffsetParameter.LeftOffset;
            LintelTypes = new List<string>(
               _revitRepository.GetLintelTypes()
               .Select(lt => lt.Name));
            SelectedLintelType = LintelTypes.FirstOrDefault(lt=>lt
            .Equals(ruleSetting.LintelTypeName, StringComparison.CurrentCultureIgnoreCase));
            OpeningWidthParameter = new OpeningWidthParameter();
            WallHalfThicknessParameter = new WallHalfThicknessParameter();
        }

        private void InitializeEmptyGroupedRule() {
            CornerParamerer = new LintelCornerParamerer();
            OpeningWidthCondition = new OpeningWidthConditionViewModel();
            LintelLeftOffsetParameter = new LintelLeftOffsetParameter();
            LintelRightOffsetParameter = new LintelRightOffsetParameter();
            OpeningWidthParameter = new OpeningWidthParameter();
            WallHalfThicknessParameter = new WallHalfThicknessParameter();
            LintelTypes = new List<string>(
                _revitRepository.GetLintelTypes()
                .Select(lt=>lt.Name));
            SelectedLintelType = LintelTypes.FirstOrDefault();
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
        }

    }
}
