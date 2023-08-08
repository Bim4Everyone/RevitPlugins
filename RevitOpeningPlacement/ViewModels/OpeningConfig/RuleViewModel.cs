using System.Collections.ObjectModel;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.ViewModels.OpeningConfig.Interfaces;



namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class RuleViewModel : BaseViewModel, ICriterionViewModel {
        private readonly Rule _rule;
        private CategoriesInfoViewModel _categoriesInfo;
        private ParameterViewModel _selectedParameter;
        private ParamValueViewModel _selectedValue;
        private ObservableCollection<RuleEvaluatorViewModel> _ruleEvaluators;
        private RuleEvaluatorViewModel _selectedRuleEvaluator;
        private ObservableCollection<ParamValueViewModel> _values;
        private string _stringValue;
        private bool _isValueEditable;

        public RuleViewModel(CategoriesInfoViewModel categoriesInfo, Rule rule = null) {
            CategoriesInfo = categoriesInfo;
            _rule = rule;

            PropertyChanged += RuleViewModelChanged;
            if(_rule != null) {
                InitializeRule();
            }
        }


        public bool IsValueEditable {
            get => _isValueEditable;
            set => RaiseAndSetIfChanged(ref _isValueEditable, value);
        }

        public ParameterViewModel SelectedParameter {
            get => _selectedParameter;
            set => RaiseAndSetIfChanged(ref _selectedParameter, value);
        }

        public CategoriesInfoViewModel CategoriesInfo {
            get => _categoriesInfo;
            set => RaiseAndSetIfChanged(ref _categoriesInfo, value);
        }

        public RuleEvaluatorViewModel SelectedRuleEvaluator {
            get => _selectedRuleEvaluator;
            set => RaiseAndSetIfChanged(ref _selectedRuleEvaluator, value);
        }

        public ObservableCollection<RuleEvaluatorViewModel> RuleEvaluators {
            get => _ruleEvaluators;
            set => RaiseAndSetIfChanged(ref _ruleEvaluators, value);
        }

        public ObservableCollection<ParamValueViewModel> Values {
            get => _values;
            set => RaiseAndSetIfChanged(ref _values, value);
        }

        public ParamValueViewModel SelectedValue {
            get => _selectedValue;
            set => RaiseAndSetIfChanged(ref _selectedValue, value);
        }

        public string StringValue {
            get => _stringValue;
            set => RaiseAndSetIfChanged(ref _stringValue, value);
        }

        public void Renew() {
            SelectedParameter = null;
            SelectedRuleEvaluator = null;
            StringValue = null;
            SelectedValue = null;
        }

        public Criterion GetCriterion() {
            return new Rule() {
                Evaluator = SelectedRuleEvaluator.RuleEvaluator,
                Provider = SelectedParameter.FilterableValueProvider.Provider,
                Value = (SelectedValue == null || SelectedValue.ParamValue == null || SelectedValue.DisplayValue != StringValue)
                ? SelectedParameter.FilterableValueProvider.Provider.GetParamValueFormString(StringValue)
                : SelectedValue.ParamValue
            };
        }

        public bool IsEmpty() {
            if(SelectedRuleEvaluator?.RuleEvaluator?.Evaluator != RevitClashDetective.Models.Evaluators.RuleEvaluators.FilterHasNoValue
                && SelectedRuleEvaluator?.RuleEvaluator?.Evaluator != RevitClashDetective.Models.Evaluators.RuleEvaluators.FilterHasValue) {
                return SelectedParameter == null || SelectedRuleEvaluator == null
                    || (SelectedValue == null && string.IsNullOrEmpty(StringValue));
            } else {
                return SelectedParameter == null || SelectedRuleEvaluator == null;
            }
        }

        public string GetErrorText() {
            if(SelectedValue != null) {
                return null;
            }
            if(SelectedRuleEvaluator.RuleEvaluator.Evaluator == RevitClashDetective.Models.Evaluators.RuleEvaluators.FilterHasValue
               || SelectedRuleEvaluator.RuleEvaluator.Evaluator == RevitClashDetective.Models.Evaluators.RuleEvaluators.FilterHasNoValue) {
                return null;
            }

            return SelectedParameter.FilterableValueProvider.GetErrorText(StringValue);
        }

        public void Initialize() {
            if(_rule != null) {
                if(!_categoriesInfo.Parameters.Any(item => item.FilterableValueProvider.Provider.Equals(_rule.Provider))) {
                    _categoriesInfo.Parameters.Add(new ParameterViewModel(_rule.Provider));
                }
                SelectedParameter = _categoriesInfo.Parameters.First(item => item.FilterableValueProvider.Provider.Equals(_rule.Provider));
                SelectedParameterChanged();
                SelectedRuleEvaluator = new RuleEvaluatorViewModel(_rule.Evaluator);
                SelectedValue = new ParamValueViewModel(_rule.Value);
                if(!Values.Contains(SelectedValue)) {
                    SelectedValue = null;
                    StringValue = _rule.Value.DisplayValue;
                }
            }
        }

        private bool CanValueEdit() {
            if(SelectedRuleEvaluator.RuleEvaluator.Evaluator == RevitClashDetective.Models.Evaluators.RuleEvaluators.FilterHasValue
               || SelectedRuleEvaluator.RuleEvaluator.Evaluator == RevitClashDetective.Models.Evaluators.RuleEvaluators.FilterHasNoValue) {
                return false;
            }

            return true;
        }

        private void InitializeRule() {
            SelectedParameter = new ParameterViewModel(_rule.Provider);
            if(!_categoriesInfo.Parameters.Contains(SelectedParameter)) {
                _categoriesInfo.Parameters.Add(SelectedParameter);
            }
            SelectedRuleEvaluator = new RuleEvaluatorViewModel(_rule.Evaluator);
            SelectedValue = new ParamValueViewModel(_rule.Value);
            StringValue = SelectedValue.ParamValue.DisplayValue;
        }

        private void RuleViewModelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if(e.PropertyName == nameof(SelectedParameter)) {
                SelectedParameterChanged();
            }
            if(e.PropertyName == nameof(SelectedRuleEvaluator)) {
                EvaluatorSelectionChanged();
            }
        }
        private void SelectedParameterChanged() {
            if(SelectedParameter != null) {
                RuleEvaluators = new ObservableCollection<RuleEvaluatorViewModel>(
                    SelectedParameter.GetEvaluators()
                        .Select(item => new RuleEvaluatorViewModel(item)));

                if((SelectedRuleEvaluator != null) && RuleEvaluators.Contains(SelectedRuleEvaluator)) {
                    EvaluatorSelectionChanged();
                } else {
                    SelectedRuleEvaluator = RuleEvaluators.FirstOrDefault();
                }
            }
        }

        private void EvaluatorSelectionChanged() {
            if(SelectedParameter != null && SelectedRuleEvaluator != null) {
                Values = new ObservableCollection<ParamValueViewModel>(
                     SelectedParameter.GetValues(
                        CategoriesInfo.Categories.ToArray(), SelectedRuleEvaluator.RuleEvaluator)
                     .Select(item => new ParamValueViewModel(item)));
                IsValueEditable = CanValueEdit();
            } else {
                Values = new ObservableCollection<ParamValueViewModel>();
            }
        }
    }
}
