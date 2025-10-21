using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.ViewModels.FilterCreatorViewModels.Interfaces;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels;
internal class SetViewModel : BaseViewModel, IСriterionViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;
    private ObservableCollection<IСriterionViewModel> _criterions;
    private CategoriesInfoViewModel _categoryInfo;
    private EvaluatorViewModel _selectedEvaluator;
    private ObservableCollection<EvaluatorViewModel> _evaluators;

    public SetViewModel(RevitRepository revitRepository,
        ILocalizationService localization,
        CategoriesInfoViewModel categoriesInfo,
        Set set = null) {
        _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new System.ArgumentNullException(nameof(localization));
        CategoryInfo = categoriesInfo ?? throw new System.ArgumentNullException(nameof(categoriesInfo));

        AddRuleCommand = RelayCommand.Create(AddRule);
        RemoveRuleCommand = RelayCommand.Create<RuleViewModel>(RemoveRule);

        AddSetCommand = RelayCommand.Create(AddSet);
        RemoveSetCommand = RelayCommand.Create<SetViewModel>(RemoveSet);

        Evaluators = new ObservableCollection<EvaluatorViewModel>(
            SetEvaluatorUtils.GetEvaluators().Select(item => new EvaluatorViewModel(_localization) {
                SetEvaluator = item
            }));
        AddRuleCommandName = _localization.GetLocalizedString("FilterCreation.AddRule");
        AddSetCommandName = _localization.GetLocalizedString("FilterCreation.AddSet");
        if(set == null) {
            Criterions = [];
            InitializeEmptyRule();
            SelectedEvaluator = Evaluators.FirstOrDefault();
        } else {
            SelectedEvaluator = Evaluators.FirstOrDefault(item => item.SetEvaluator.Evaluator == set.SetEvaluator.Evaluator);
            InitializeCriterions(set.Criteria);
        }
    }

    public ICommand AddRuleCommand { get; }
    public ICommand AddSetCommand { get; }
    public ICommand RemoveSetCommand { get; }
    public ICommand RemoveRuleCommand { get; }
    public string AddRuleCommandName { get; }
    public string AddSetCommandName { get; }

    public CategoriesInfoViewModel CategoryInfo {
        get => _categoryInfo;
        set => RaiseAndSetIfChanged(ref _categoryInfo, value);
    }

    public ObservableCollection<IСriterionViewModel> Criterions {
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
        Criterions.Add(new RuleViewModel(_revitRepository, _localization, _categoryInfo));
    }

    private void InitializeCriterions(IEnumerable<Criterion> criterions) {
        Criterions =
        [
            .. criterions.OfType<Set>().Select(item => new SetViewModel(_revitRepository, _localization, _categoryInfo, item)),
            .. criterions.OfType<Rule>().Select(item => new RuleViewModel(_revitRepository,_localization, _categoryInfo, item)),
        ];
    }

    private void AddRule() {
        Criterions.Add(new RuleViewModel(_revitRepository, _localization, _categoryInfo));
    }

    private void AddSet() {
        Criterions.Add(new SetViewModel(_revitRepository, _localization, _categoryInfo));
    }

    private void RemoveSet(SetViewModel p) {
        Criterions.Remove(p);
    }

    private void RemoveRule(RuleViewModel p) {
        Criterions.Remove(p);
    }

    public void Renew() {
        foreach(var criterion in Criterions) {
            criterion.Renew();
        }
    }

    public Criterion GetCriterion() {
        return new Set() {
            SetEvaluator = SelectedEvaluator.SetEvaluator,
            Criteria = Criterions.Select(item => item.GetCriterion()).ToList(),
            RevitRepository = _revitRepository
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
