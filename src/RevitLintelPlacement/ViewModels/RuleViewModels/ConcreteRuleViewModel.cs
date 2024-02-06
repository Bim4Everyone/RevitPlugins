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
        private readonly ElementInfosViewModel _elementInfos;
        private readonly List<IConditionViewModel> _conditions = new List<IConditionViewModel>();
        private string _selectedLintelType;
        private OpeningWidthConditionViewModel _openingWidthCondition;
        private LintelLeftOffsetParameter _lintelLeftOffsetParameter;
        private LintelRightOffsetParameter _lintelRightOffsetParameter;
        private List<string> _lintelTypes;
        private List<ILintelParameterViewModel> _parameters = new List<ILintelParameterViewModel>();

        public ConcreteRuleViewModel() {
        }

        public ConcreteRuleViewModel(RevitRepository revitRepository, ElementInfosViewModel elementInfos, RuleSetting ruleSetting = null) {
            this._revitRepository = revitRepository;
            this._elementInfos = elementInfos;
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

        //public LintelCornerParameter CornerParameter { get; set; }

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
            LintelRightOffsetParameter.RightOffset = LintelLeftOffsetParameter.LeftOffset;
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
                if (parameter is LintelLeftOffsetParameter leftOffsetParameter) {
                    yield return new LintelParameterSetting() {
                        LintelParameterType = LintelParameterType.LeftOffsetParameter,
                        Offset = leftOffsetParameter.LeftOffset
                    };
                    yield return new LintelParameterSetting() {
                        LintelParameterType = LintelParameterType.RightOffsetParameter,
                        Offset = leftOffsetParameter.LeftOffset
                    };
                }
                //if (parameter is LintelCornerParameter cornerParameter) {
                //    yield return new LintelParameterSetting() {
                //        LintelParameterType = LintelParameterType.CornerParameter,
                //        IsCornerChecked = cornerParameter.IsCornerChecked
                //    };
                //}
            }
        }

        private void InitializeGroupedRule(RuleSetting ruleSetting) {

            foreach(var condition in ruleSetting.ConditionSettingsConfig) {
                switch(condition.ConditionType) {
                    case ConditionType.OpeningWidth: {
                        OpeningWidthCondition = new OpeningWidthConditionViewModel(_revitRepository, _elementInfos) {
                            MaxWidth = condition.OpeningWidthMax,
                            MinWidth = condition.OpeningWidthMin
                        };
                        break;
                    }
                }
            }

            foreach(var parameter in ruleSetting.LintelParameterSettingsConfig) {
                switch(parameter.LintelParameterType) {
                    //case LintelParameterType.CornerParameter:
                    //CornerParameter = new LintelCornerParameter() {
                    //    IsCornerChecked = parameter.IsCornerChecked
                    //};
                    //break;
                    case LintelParameterType.LeftOffsetParameter: {
                        LintelLeftOffsetParameter = new LintelLeftOffsetParameter(_revitRepository) {
                            LeftOffset = parameter.Offset
                        };
                        break;
                    }
                    case LintelParameterType.RightOffsetParameter: {
                        LintelRightOffsetParameter = new LintelRightOffsetParameter(_revitRepository) {
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
            if(string.IsNullOrEmpty(SelectedLintelType)) {
                SelectedLintelType = LintelTypes.FirstOrDefault();
            }
            OpeningWidthParameter = new OpeningWidthParameter(_revitRepository, _elementInfos);
            WallHalfThicknessParameter = new WallHalfThicknessParameter(_revitRepository);
        }

        private void InitializeEmptyGroupedRule() {
            //CornerParameter = new LintelCornerParameter();
            OpeningWidthCondition = new OpeningWidthConditionViewModel(_revitRepository, _elementInfos);
            LintelLeftOffsetParameter = new LintelLeftOffsetParameter(_revitRepository);
            LintelRightOffsetParameter = new LintelRightOffsetParameter(_revitRepository);
            OpeningWidthParameter = new OpeningWidthParameter(_revitRepository, _elementInfos);
            WallHalfThicknessParameter = new WallHalfThicknessParameter(_revitRepository);
            LintelTypes = new List<string>(
                _revitRepository.GetLintelTypes()
                .Select(lt=>lt.Name));
            SelectedLintelType = LintelTypes.FirstOrDefault();
        }

        private void AddParameters() {
            _parameters = new List<ILintelParameterViewModel> {
                //CornerParameter,
                OpeningWidthParameter,
                WallHalfThicknessParameter,
                LintelRightOffsetParameter,
                LintelLeftOffsetParameter
            };
        }

        private void AddConditions() {
            _conditions.Add(OpeningWidthCondition);
        }

    }
}
