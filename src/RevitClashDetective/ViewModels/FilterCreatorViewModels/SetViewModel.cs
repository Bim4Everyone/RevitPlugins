using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.ViewModels.FilterCreatorViewModels.Interfaces;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class SetViewModel : BaseViewModel, IСriterionViewModel {
        private readonly RevitRepository _revitRepository;
        private ObservableCollection<IСriterionViewModel> _criterions;
        private CategoriesInfoViewModel _categoryInfo;
        private EvaluatorViewModel _selectedEvaluator;
        private ObservableCollection<EvaluatorViewModel> _evaluators;

        public SetViewModel(RevitRepository revitRepository, CategoriesInfoViewModel categoriesInfo, Set set = null) {
            _revitRepository = revitRepository;

            CategoryInfo = categoriesInfo;

            AddRuleCommand = RelayCommand.Create(AddRule);
            RemoveRuleCommand = RelayCommand.Create<RuleViewModel>(RemoveRule);

            AddSetCommand = RelayCommand.Create(AddSet);
            RemoveSetCommand = RelayCommand.Create<SetViewModel>(RemoveSet);

            Evaluators = new ObservableCollection<EvaluatorViewModel>(SetEvaluatorUtils.GetEvaluators().Select(item => new EvaluatorViewModel() { SetEvaluator = item }));
            if(set == null) {
                Criterions = new ObservableCollection<IСriterionViewModel>();
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

        public CategoriesInfoViewModel CategoryInfo {
            get => _categoryInfo;
            set => this.RaiseAndSetIfChanged(ref _categoryInfo, value);
        }

        public ObservableCollection<IСriterionViewModel> Criterions {
            get => _criterions;
            set => this.RaiseAndSetIfChanged(ref _criterions, value);
        }

        public EvaluatorViewModel SelectedEvaluator {
            get => _selectedEvaluator;
            set => this.RaiseAndSetIfChanged(ref _selectedEvaluator, value);
        }

        public ObservableCollection<EvaluatorViewModel> Evaluators {
            get => _evaluators;
            set => this.RaiseAndSetIfChanged(ref _evaluators, value);
        }

        public void InitializeEmptyRule() {
            Criterions.Add(new RuleViewModel(_revitRepository, _categoryInfo));
        }

        private void InitializeCriterions(IEnumerable<Criterion> criterions) {
            Criterions = new ObservableCollection<IСriterionViewModel>();
            foreach(var set in criterions.OfType<Set>().Select(item => new SetViewModel(_revitRepository, _categoryInfo, item))) {
                Criterions.Add(set);
            }
            foreach(var rule in criterions.OfType<Rule>().Select(item => new RuleViewModel(_revitRepository, _categoryInfo, item))) {
                Criterions.Add(rule);
            }
        }

        private void AddRule() {
            Criterions.Add(new RuleViewModel(_revitRepository, _categoryInfo));
        }

        private void AddSet() {
            Criterions.Add(new SetViewModel(_revitRepository, _categoryInfo));
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
}
