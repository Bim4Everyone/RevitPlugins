using System.Collections.ObjectModel;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.ViewModels.OpeningConfig.Interfaces;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
internal class RuleViewModel : BaseViewModel, ICriterionViewModel {
    private readonly ILocalizationService _localization;
    private readonly Rule _rule;
    private CategoriesInfoViewModel _categoriesInfo;
    private ParameterViewModel _selectedParameter;
    private ParamValueViewModel _selectedValue;
    private ObservableCollection<RuleEvaluatorViewModel> _ruleEvaluators;
    private RuleEvaluatorViewModel _selectedRuleEvaluator;
    private ObservableCollection<ParamValueViewModel> _values;
    private string _stringValue;
    private bool _isValueEditable;

    public RuleViewModel(ILocalizationService localization,
        CategoriesInfoViewModel categoriesInfo,
        Rule rule = null) {
        _localization = localization ?? throw new System.ArgumentNullException(nameof(localization));
        CategoriesInfo = categoriesInfo ?? throw new System.ArgumentNullException(nameof(categoriesInfo));
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
        return SelectedRuleEvaluator?.RuleEvaluator?.Evaluator is not RevitClashDetective.Models.Evaluators.RuleEvaluators.FilterHasNoValue
            and not RevitClashDetective.Models.Evaluators.RuleEvaluators.FilterHasValue
            ? SelectedParameter == null || SelectedRuleEvaluator == null
                || (SelectedValue == null && string.IsNullOrEmpty(StringValue))
            : SelectedParameter == null || SelectedRuleEvaluator == null;
    }

    public string GetErrorText() {
        return SelectedValue != null
            ? null
            : SelectedRuleEvaluator.RuleEvaluator.Evaluator is RevitClashDetective.Models.Evaluators.RuleEvaluators.FilterHasValue
           or RevitClashDetective.Models.Evaluators.RuleEvaluators.FilterHasNoValue
            ? null
            : SelectedParameter.FilterableValueProvider.GetErrorText(StringValue);
    }

    public void Initialize() {
        if(_rule != null) {
            if(!_categoriesInfo.Parameters.Any(item => item.FilterableValueProvider.Provider.Equals(_rule.Provider))) {
                _categoriesInfo.Parameters.Add(new ParameterViewModel(_localization, _rule.Provider));
            }
            SelectedParameter = _categoriesInfo.Parameters.First(item => item.FilterableValueProvider.Provider.Equals(_rule.Provider));
            SelectedParameterChanged();
            SelectedRuleEvaluator = new RuleEvaluatorViewModel(_localization, _rule.Evaluator);
            SelectedValue = new ParamValueViewModel(_rule.Value);
            if(!Values.Contains(SelectedValue)) {
                SelectedValue = null;
                StringValue = _rule.Value.DisplayValue;
            }
        }
    }

    private bool CanValueEdit() {
        return SelectedRuleEvaluator.RuleEvaluator.Evaluator is not RevitClashDetective.Models.Evaluators.RuleEvaluators.FilterHasValue
           and not RevitClashDetective.Models.Evaluators.RuleEvaluators.FilterHasNoValue;
    }

    private void InitializeRule() {
        SelectedParameter = new ParameterViewModel(_localization, _rule.Provider);
        if(!_categoriesInfo.Parameters.Contains(SelectedParameter)) {
            _categoriesInfo.Parameters.Add(SelectedParameter);
        }
        SelectedRuleEvaluator = new RuleEvaluatorViewModel(_localization, _rule.Evaluator);
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
                    .Select(item => new RuleEvaluatorViewModel(_localization, item)));

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
            Values = [];
        }
    }
}
