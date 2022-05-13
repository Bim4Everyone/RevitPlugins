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
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.ViewModels.FilterCreatorViewModels.Interfaces;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class SetViewModel : BaseViewModel, IСriterionViewModel {
        private readonly RevitRepository _revitRepository;
        private ObservableCollection<IСriterionViewModel> _criterions;
        private CategoriesInfoViewModel _categoryInfo;
        private EvaluatorViewModel _selectedEvaluator;
        private ObservableCollection<EvaluatorViewModel> _evaluators;

        public SetViewModel(RevitRepository revitRepository, CategoriesInfoViewModel categoriesInfo) {
            _revitRepository = revitRepository;

            CategoryInfo = categoriesInfo;

            AddRuleCommand = new RelayCommand(AddRule);
            AddSetCommand = new RelayCommand(AddSet);
            RemoveRuleCommand = new RelayCommand(RemoveRule);
            RemoveSetCommand = new RelayCommand(RemoveSet);

            Criterions = new ObservableCollection<IСriterionViewModel>();
            InitializeEmtyRule();
            Evaluators = new ObservableCollection<EvaluatorViewModel>(SetEvaluatorUtils.GetEvaluators().Select(item => new EvaluatorViewModel() { SetEvaluator = item }));
            SelectedEvaluator = Evaluators.FirstOrDefault();


        }

        public ICommand AddRuleCommand { get; }
        public ICommand AddSetCommand { get; }
        public ICommand RemoveSetCommand { get; }
        public ICommand RemoveRuleCommand { get; }
        public ICommand SelectionChangedCommand { get; }

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

        public void InitializeEmtyRule() {
            Criterions.Add(new RuleViewModel(_revitRepository, _categoryInfo));
        }

        private void AddRule(object p) {
            Criterions.Add(new RuleViewModel(_revitRepository, _categoryInfo));
        }

        private void AddSet(object p) {
            Criterions.Add(new SetViewModel(_revitRepository, _categoryInfo));
        }

        private void RemoveSet(object p) {
            if(p is SetViewModel set) {
                Criterions.Remove(set);
            }
        }

        private void RemoveRule(object p) {
            if(p is RuleViewModel rule) {
                Criterions.Remove(rule);
            }
        }

        public void Renew() {
            foreach(var criterion in Criterions) {
                criterion.Renew();
            }
        }

        public ICriterion GetCriterion() {
            return new Set() {
                SetEvaluator = SelectedEvaluator.SetEvaluator,
                Criteria = Criterions.Select(item => item.GetCriterion()).ToList()
            };
        }

        public bool IsEmty() {
            return Criterions.Any(item => item.IsEmty());
        }
    }
}