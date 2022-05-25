using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterableValueProviders;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.Value;
using RevitClashDetective.ViewModels.FilterCreatorViewModels.Interfaces;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class RuleViewModel : BaseViewModel, IСriterionViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly Rule _rule;
        private CategoriesInfoViewModel _categoriesInfo;
        private ParameterViewModel _selectedParameter;
        private ParamValueViewModel _selectedValue;
        private ObservableCollection<RuleEvaluatorViewModel> _ruleEvaluators;
        private RuleEvaluatorViewModel _selectedRuleEvaluator;
        private ObservableCollection<ParamValueViewModel> _values;
        private string _stringValue;
        private bool _isValueEditable;
        private bool isStringValueWrong;

        public RuleViewModel(RevitRepository revitRepository, CategoriesInfoViewModel categoriesInfo, Rule rule = null) {
            _revitRepository = revitRepository;
            CategoriesInfo = categoriesInfo;
            _rule = rule;
            ParameterSelectionChangedCommand = new RelayCommand(ParameterSelectionChanged);
            EvaluatorSelectionChangedCommand = new RelayCommand(EvaluatorSelectionChanged);

            if(_rule != null) {
                InitializeRule();
            }
        }

        public ICommand ParameterSelectionChangedCommand { get; set; }
        public ICommand EvaluatorSelectionChangedCommand { get; set; }

        public bool IsValueEditable {
            get => _isValueEditable;
            set => this.RaiseAndSetIfChanged(ref _isValueEditable, value);
        }

        public ParameterViewModel SelectedParameter {
            get => _selectedParameter;
            set => this.RaiseAndSetIfChanged(ref _selectedParameter, value);
        }

        public CategoriesInfoViewModel CategoriesInfo {
            get => _categoriesInfo;
            set => this.RaiseAndSetIfChanged(ref _categoriesInfo, value);
        }

        public RuleEvaluatorViewModel SelectedRuleEvaluator {
            get => _selectedRuleEvaluator;
            set => this.RaiseAndSetIfChanged(ref _selectedRuleEvaluator, value);
        }

        public ObservableCollection<RuleEvaluatorViewModel> RuleEvaluators {
            get => _ruleEvaluators;
            set => this.RaiseAndSetIfChanged(ref _ruleEvaluators, value);
        }

        public ObservableCollection<ParamValueViewModel> Values {
            get => _values;
            set => this.RaiseAndSetIfChanged(ref _values, value);
        }

        public ParamValueViewModel SelectedValue {
            get => _selectedValue;
            set => this.RaiseAndSetIfChanged(ref _selectedValue, value);
        }

        public string StringValue {
            get => _stringValue;
            set => this.RaiseAndSetIfChanged(ref _stringValue, value);
        }

        public void Renew() {
            SelectedParameter = null;
            SelectedRuleEvaluator = null;
        }

        private void InitializeRule() {
            SelectedParameter = new ParameterViewModel(_rule.Provider);
            if(!_categoriesInfo.Parameters.Contains(SelectedParameter)) {
                _categoriesInfo.Parameters.Add(SelectedParameter);
            }
            SelectedRuleEvaluator = new RuleEvaluatorViewModel(_rule.Evaluator);
            SelectedValue = new ParamValueViewModel(_rule.Value);
        }



        private void ParameterSelectionChanged(object p) {
            if(SelectedParameter != null) {
                RuleEvaluators = new ObservableCollection<RuleEvaluatorViewModel>(
                    SelectedParameter.GetEvaluators()
                        .Select(item => new RuleEvaluatorViewModel(item)));

                SelectedRuleEvaluator = SelectedRuleEvaluator ?? RuleEvaluators.FirstOrDefault();
            }
        }

        private void EvaluatorSelectionChanged(object p) {
            if(SelectedParameter != null && SelectedRuleEvaluator != null) {
                Values = new ObservableCollection<ParamValueViewModel>(
                     SelectedParameter.GetValues(
                        CategoriesInfo.Categories
                            .Select(item => item.Category), SelectedRuleEvaluator.RuleEvaluator));
                StringValue = null;
                IsValueEditable = CanValueEdit();
            } else {
                Values = new ObservableCollection<ParamValueViewModel>();
            }
        }

        public Criterion GetCriterion() {
            return new Rule() {
                Evaluator = SelectedRuleEvaluator.RuleEvaluator,
                Provider = SelectedParameter.FilterableValueProvider,
                Value = SelectedValue?.ParamValue ?? SelectedParameter.FilterableValueProvider.GetParamValue(StringValue)
            };
        }

        public bool IsEmpty() {
            return SelectedParameter == null || SelectedRuleEvaluator == null ||
                   (SelectedValue == null && StringValue == null);
        }

        private bool CanValueEdit() {
            if(SelectedRuleEvaluator.RuleEvaluator.Evaluator == Models.Evaluators.RuleEvaluators.FilterHasValue
               || SelectedRuleEvaluator.RuleEvaluator.Evaluator == Models.Evaluators.RuleEvaluators.FilterHasNoValue) {
                return false;
            }

            if((SelectedRuleEvaluator.RuleEvaluator.Evaluator == Models.Evaluators.RuleEvaluators.FilterNotEquals
                || SelectedRuleEvaluator.RuleEvaluator.Evaluator == Models.Evaluators.RuleEvaluators.FilterNumericEquals
                || SelectedRuleEvaluator.RuleEvaluator.Evaluator == Models.Evaluators.RuleEvaluators.FilterStringEquals)
               && SelectedParameter.FilterableValueProvider is ParameterValueProvider provider
               && provider.RevitParam.StorageType == Autodesk.Revit.DB.StorageType.ElementId) {
                return false;
            }

            return true;
        }

        public string GetErrorText() {
            if(SelectedValue != null) {
                return null;
            }

            return SelectedParameter.FilterableValueProvider.GetErrorText(StringValue);
        }

        public void Initialize() {
            if(_rule != null) {
                SelectedParameter = new ParameterViewModel(_rule.Provider);
                if(!_categoriesInfo.Parameters.Contains(SelectedParameter)) {
                    _categoriesInfo.Parameters.Add(SelectedParameter);
                }
                ParameterSelectionChanged(null);
                SelectedRuleEvaluator = new RuleEvaluatorViewModel(_rule.Evaluator);
                EvaluatorSelectionChanged(null);
                SelectedValue = new ParamValueViewModel(_rule.Value);
                if(!Values.Contains(SelectedValue)) {
                    SelectedValue = null;
                    StringValue = _rule.Value.DisplayValue;
                }
            }
        }
    }
}