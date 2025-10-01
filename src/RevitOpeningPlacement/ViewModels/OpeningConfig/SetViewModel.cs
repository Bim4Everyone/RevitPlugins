using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.ViewModels.OpeningConfig.Interfaces;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
internal class SetViewModel : BaseViewModel, ICriterionViewModel {
    private readonly RevitRepository _revitRepository;
    private ObservableCollection<ICriterionViewModel> _criterions;
    private CategoriesInfoViewModel _categoryInfo;
    private EvaluatorViewModel _selectedEvaluator;
    private ObservableCollection<EvaluatorViewModel> _evaluators;

    public SetViewModel(RevitRepository revitRepository, CategoriesInfoViewModel categoriesInfo, Set set = null) {
        _revitRepository = revitRepository;
        CategoryInfo = categoriesInfo;

        AddRuleCommand = RelayCommand.Create(AddRule);
        RemoveRuleCommand = RelayCommand.Create<RuleViewModel>(RemoveRule, CanRemoveRule);

        AddSetCommand = RelayCommand.Create(AddSet);
        RemoveSetCommand = RelayCommand.Create<SetViewModel>(RemoveSet, CanRemoveSet);

        Evaluators = new ObservableCollection<EvaluatorViewModel>(SetEvaluatorUtils.GetEvaluators().Select(item => new EvaluatorViewModel() { SetEvaluator = item }));
        if(set == null) {
            Criterions = [];
            SelectedEvaluator = Evaluators.FirstOrDefault();
        } else {
            SelectedEvaluator = Evaluators.FirstOrDefault(item => item.SetEvaluator.Evaluator == set.SetEvaluator?.Evaluator);
            InitializeCriterions(set.Criteria);
        }
    }

    public ICommand AddRuleCommand { get; }
    public ICommand AddSetCommand { get; }
    public ICommand RemoveSetCommand { get; }
    public ICommand RemoveRuleCommand { get; }

    public CategoriesInfoViewModel CategoryInfo {
        get => _categoryInfo;
        set => RaiseAndSetIfChanged(ref _categoryInfo, value);
    }

    public ObservableCollection<ICriterionViewModel> Criterions {
        get => _criterions;
        set => RaiseAndSetIfChanged(ref _criterions, value);
    }

    public EvaluatorViewModel SelectedEvaluator {
        get => _selectedEvaluator;
        set => RaiseAndSetIfChanged(ref _selectedEvaluator, value);
    }

    public ObservableCollection<EvaluatorViewModel> Evaluators {
        get => _evaluators;
        set => RaiseAndSetIfChanged(ref _evaluators, value);
    }

    public void InitializeEmptyRule() {
        Criterions.Add(new RuleViewModel(_categoryInfo));
    }

    private void InitializeCriterions(IEnumerable<Criterion> criterions) {
        Criterions = [];
        SetCriterionsRevitRepository(criterions);
        foreach(var set in criterions.OfType<Set>().Select(item => new SetViewModel(_revitRepository, _categoryInfo, item))) {
            Criterions.Add(set);
        }
        foreach(var rule in criterions.OfType<Rule>().Select(item => new RuleViewModel(_categoryInfo, item))) {
            Criterions.Add(rule);
        }
    }

    private void SetCriterionsRevitRepository(IEnumerable<Criterion> criterions) {
        foreach(var criterion in criterions) {
            criterion.SetRevitRepository(_revitRepository.GetClashRevitRepository());
        }
    }

    private void AddRule() {
        Criterions.Add(new RuleViewModel(_categoryInfo));
    }

    private void AddSet() {
        Criterions.Add(new SetViewModel(_revitRepository, _categoryInfo));
    }

    private void RemoveSet(SetViewModel p) {
        Criterions.Remove(p);
    }

    private bool CanRemoveSet(SetViewModel p) {
        return p != null;
    }

    private void RemoveRule(RuleViewModel p) {
        Criterions.Remove(p);
    }

    private bool CanRemoveRule(RuleViewModel p) {
        return p != null;
    }

    public void Renew() {
        foreach(var criterion in Criterions) {
            criterion.Renew();
        }
    }

    public Criterion GetCriterion() {
        return GetSet();
    }

    public Set GetSet() {
        return new Set() {
            SetEvaluator = SelectedEvaluator.SetEvaluator,
            Criteria = Criterions.Select(item => item.GetCriterion()).ToList()
        };
    }

    public bool IsEmpty() {
        return Criterions.Any(item => item.IsEmpty()) || _categoryInfo.Categories.Count == 0;
    }

    public string GetErrorText() {
        return Criterions.FirstOrDefault(item => item.GetErrorText() != null)?.GetErrorText();
    }

    public void Initialize() {
        foreach(var criterion in Criterions) {
            criterion.Initialize();
        }
    }
}
