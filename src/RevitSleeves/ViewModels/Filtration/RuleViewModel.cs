using System;
using System.Collections.ObjectModel;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.FilterModel;

namespace RevitSleeves.ViewModels.Filtration;
internal class RuleViewModel : BaseViewModel, ICriterionViewModel {
    private const RevitClashDetective.Models.Evaluators.RuleEvaluators _hasNoValue
        = RevitClashDetective.Models.Evaluators.RuleEvaluators.FilterHasNoValue;
    private const RevitClashDetective.Models.Evaluators.RuleEvaluators _hasValue
        = RevitClashDetective.Models.Evaluators.RuleEvaluators.FilterHasValue;
    private readonly ILocalizationService _localizationService;
    private readonly Rule _rule;
    private ParameterViewModel _selectedParameter;
    private RuleEvaluatorViewModel _selectedRuleEvaluator;
    private ParamValueViewModel _selectedValue;
    private ObservableCollection<ParamValueViewModel> _values;
    private ObservableCollection<RuleEvaluatorViewModel> _ruleEvaluators;
    private string _displayValue;
    private bool _isValueEditable;

    public RuleViewModel(ILocalizationService localizationService,
        CategoryInfoViewModel categoryInfoViewModel,
        Rule rule = null) {

        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        CategoryInfo = categoryInfoViewModel ?? throw new ArgumentNullException(nameof(categoryInfoViewModel));
        _rule = rule;

        PropertyChanged += RuleViewModelChanged;
        if(_rule != null) {
            InitializeRule();
        }
    }


    public CategoryInfoViewModel CategoryInfo { get; }

    public ParameterViewModel SelectedParameter {
        get => _selectedParameter;
        set => RaiseAndSetIfChanged(ref _selectedParameter, value);
    }

    public RuleEvaluatorViewModel SelectedRuleEvaluator {
        get => _selectedRuleEvaluator;
        set => RaiseAndSetIfChanged(ref _selectedRuleEvaluator, value);
    }

    public ParamValueViewModel SelectedValue {
        get => _selectedValue;
        set => RaiseAndSetIfChanged(ref _selectedValue, value);
    }

    public ObservableCollection<ParamValueViewModel> Values {
        get => _values;
        set => RaiseAndSetIfChanged(ref _values, value);
    }

    public ObservableCollection<RuleEvaluatorViewModel> RuleEvaluators {
        get => _ruleEvaluators;
        set => RaiseAndSetIfChanged(ref _ruleEvaluators, value);
    }

    public string DisplayValue {
        get => _displayValue;
        set => RaiseAndSetIfChanged(ref _displayValue, value);
    }

    public bool IsValueEditable {
        get => _isValueEditable;
        set => RaiseAndSetIfChanged(ref _isValueEditable, value);
    }


    public Criterion GetCriterion() {
        return new Rule() {
            Evaluator = SelectedRuleEvaluator.RuleEvaluator,
            Provider = SelectedParameter.ProviderViewModel.Provider,
            Value = (SelectedValue?.ParamValue is null || SelectedValue.DisplayValue != DisplayValue)
                ? SelectedParameter.ProviderViewModel.Provider.GetParamValueFormString(DisplayValue)
                : SelectedValue.ParamValue
        };
    }

    public string GetErrorText() {
        if(SelectedValue != null) {
            return string.Empty;
        }
        if(SelectedRuleEvaluator.RuleEvaluator.Evaluator == _hasValue
            || SelectedRuleEvaluator.RuleEvaluator.Evaluator == _hasNoValue) {
            return string.Empty;
        }

        return SelectedParameter.ProviderViewModel.GetErrorText(DisplayValue);
    }

    public void Initialize() {
        if(_rule != null) {
            SelectedParameter = CategoryInfo.Parameters.First(
                item => item.ProviderViewModel.Provider.Equals(_rule.Provider));
            SelectedParameterChanged();
            SelectedRuleEvaluator = new RuleEvaluatorViewModel(_localizationService, _rule.Evaluator);
            SelectedValue = new ParamValueViewModel(_rule.Value);
            if(!Values.Contains(SelectedValue)) {
                SelectedValue = null;
                DisplayValue = _rule.Value.DisplayValue;
            }
        }
    }

    public bool IsEmpty() {
        if(SelectedRuleEvaluator?.RuleEvaluator?.Evaluator != _hasNoValue
            && SelectedRuleEvaluator?.RuleEvaluator?.Evaluator != _hasValue) {
            return SelectedParameter == null
                || SelectedRuleEvaluator == null
                || (SelectedValue == null && string.IsNullOrEmpty(DisplayValue));
        } else {
            return SelectedParameter == null || SelectedRuleEvaluator == null;
        }
    }

    public void Renew() {
        SelectedParameter = null;
        SelectedRuleEvaluator = null;
        DisplayValue = null;
        SelectedValue = null;
    }

    private void RuleViewModelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(SelectedParameter)) {
            SelectedParameterChanged();
        }
        if(e.PropertyName == nameof(SelectedRuleEvaluator)) {
            SelectedEvaluatorChanged();
        }
    }

    private void SelectedParameterChanged() {
        if(SelectedParameter != null) {
            RuleEvaluators = new ObservableCollection<RuleEvaluatorViewModel>(
                SelectedParameter
                .GetEvaluators()
                .Select(item => new RuleEvaluatorViewModel(_localizationService, item)));

            if((SelectedRuleEvaluator != null) && RuleEvaluators.Contains(SelectedRuleEvaluator)) {
                SelectedEvaluatorChanged();
            } else {
                SelectedRuleEvaluator = RuleEvaluators.FirstOrDefault();
            }
        }
    }

    private void SelectedEvaluatorChanged() {
        if(SelectedParameter != null && SelectedRuleEvaluator != null) {
            Values = new ObservableCollection<ParamValueViewModel>(
                 SelectedParameter.GetValues(CategoryInfo.Category, SelectedRuleEvaluator.RuleEvaluator)
                 .Select(item => new ParamValueViewModel(item)));
            IsValueEditable = CanValueEdit();
        } else {
            Values = [];
        }
    }

    private bool CanValueEdit() {
        var selectedRuleEvaluator = SelectedRuleEvaluator.RuleEvaluator.Evaluator;
        return !(selectedRuleEvaluator == _hasValue || selectedRuleEvaluator == _hasNoValue);
    }

    private void InitializeRule() {
        SelectedParameter = new ParameterViewModel(_localizationService, _rule.Provider);
        SelectedRuleEvaluator = new RuleEvaluatorViewModel(_localizationService, _rule.Evaluator);
        SelectedValue = new ParamValueViewModel(_rule.Value);
        DisplayValue = SelectedValue.ParamValue.DisplayValue;
    }
}
