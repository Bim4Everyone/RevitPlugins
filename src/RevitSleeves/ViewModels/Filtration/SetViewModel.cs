using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterModel;

using RevitSleeves.Models;

namespace RevitSleeves.ViewModels.Filtration;
internal class SetViewModel : BaseViewModel, ICriterionViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private SetEvaluatorViewModel _selectedEvaluator;
    private ObservableCollection<ICriterionViewModel> _criteria;

    public SetViewModel(
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        CategoryInfoViewModel categoryInfo,
        Set set = null) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        CategoryInfo = categoryInfo ?? throw new ArgumentNullException(nameof(categoryInfo));
        Evaluators = new ReadOnlyCollection<SetEvaluatorViewModel>(
            [.. SetEvaluatorUtils.GetEvaluators().Select(i => new SetEvaluatorViewModel(_localizationService, i))]);

        AddRuleCommand = RelayCommand.Create(AddRule);
        RemoveRuleCommand = RelayCommand.Create<RuleViewModel>(RemoveRule, CanRemoveRule);
        AddSetCommand = RelayCommand.Create(AddSet);
        RemoveSetCommand = RelayCommand.Create<SetViewModel>(RemoveSet, CanRemoveSet);

        AddRuleCommandName = _localizationService.GetLocalizedString("Filtering.AddRule");
        AddSetCommandName = _localizationService.GetLocalizedString("Filtering.AddSet");

        if(set == null) {
            Criteria = [];
            SelectedEvaluator = Evaluators.FirstOrDefault();
        } else {
            SelectedEvaluator = Evaluators.FirstOrDefault(
                item => item.SetEvaluator.Evaluator == set.SetEvaluator?.Evaluator);
            InitializeCriteria(set.Criteria);
        }
    }


    public ICommand AddRuleCommand { get; }

    public ICommand AddSetCommand { get; }

    public ICommand RemoveSetCommand { get; }

    public ICommand RemoveRuleCommand { get; }

    public CategoryInfoViewModel CategoryInfo { get; }

    public ObservableCollection<ICriterionViewModel> Criteria {
        get => _criteria;
        set => RaiseAndSetIfChanged(ref _criteria, value);
    }

    public SetEvaluatorViewModel SelectedEvaluator {
        get => _selectedEvaluator;
        set => RaiseAndSetIfChanged(ref _selectedEvaluator, value);
    }

    public IReadOnlyCollection<SetEvaluatorViewModel> Evaluators { get; }

    public string AddRuleCommandName { get; }

    public string AddSetCommandName { get; }


    public Set GetSet() {
        return new Set() {
            SetEvaluator = SelectedEvaluator.SetEvaluator,
            Criteria = [.. Criteria.Select(item => item.GetCriterion())]
        };
    }

    public Criterion GetCriterion() {
        return GetSet();
    }

    public string GetErrorText() {
        return Criteria.FirstOrDefault(item => item.GetErrorText() != null)?.GetErrorText();
    }

    public void Initialize() {
        foreach(var criterion in Criteria) {
            criterion.Initialize();
        }
    }

    public bool IsEmpty() {
        return Criteria.Any(item => item.IsEmpty());
    }

    public void Renew() {
        foreach(var criterion in Criteria) {
            criterion.Renew();
        }
    }

    private void InitializeCriteria(ICollection<Criterion> criteria) {
        Criteria = [];
        SetCriteriaRepository(criteria);
        foreach(var set in criteria.OfType<Set>()
            .Select(item => new SetViewModel(_revitRepository, _localizationService, CategoryInfo, item))) {
            Criteria.Add(set);
        }
        foreach(var rule in criteria.OfType<Rule>()
            .Select(item => new RuleViewModel(_localizationService, CategoryInfo, item))) {
            Criteria.Add(rule);
        }
    }

    private void SetCriteriaRepository(ICollection<Criterion> criteria) {
        foreach(var criterion in criteria) {
            criterion.SetRevitRepository(_revitRepository.GetClashRevitRepository());
        }
    }

    private void AddSet() {
        Criteria.Add(new SetViewModel(_revitRepository, _localizationService, CategoryInfo));
    }

    private void RemoveSet(SetViewModel model) {
        Criteria.Remove(model);
    }

    private bool CanRemoveSet(SetViewModel model) {
        return model is not null;
    }

    private void AddRule() {
        Criteria.Add(new RuleViewModel(_localizationService, CategoryInfo));
    }

    private bool CanRemoveRule(RuleViewModel model) {
        return model is not null;
    }

    private void RemoveRule(RuleViewModel model) {
        Criteria.Remove(model);
    }
}
