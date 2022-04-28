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
using RevitClashDetective.ViewModels.FilterCreatorViewModels.Interfaces;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class SetViewModel : BaseViewModel, IСriterionViewModel {
        private readonly RevitRepository _revitRepository;
        private ObservableCollection<BaseViewModel> _criterions;
        private CategoriesInfoViewModel _categoryInfo;
        private EvaluatorViewModel _selectedEvaluator;
        private ObservableCollection<EvaluatorViewModel> _evaluators;

        public SetViewModel(RevitRepository revitRepository, CategoriesInfoViewModel categoriesInfo) {
            _revitRepository = revitRepository;

            CategoryInfo = categoriesInfo;

            AddRuleCommand = new RelayCommand(AddRule);
            AddSetCommand = new RelayCommand(AddSet);

            Criterions = new ObservableCollection<BaseViewModel>();
            Evaluators = new ObservableCollection<EvaluatorViewModel>() {
            new EvaluatorViewModel(){Name="И"},
            new EvaluatorViewModel(){Name="ИЛИ"} };
            SelectedEvaluator = Evaluators.FirstOrDefault();
            InitializeEmtyRule();
        }

        public ICommand AddRuleCommand { get; }
        public ICommand AddSetCommand { get; }

        public CategoriesInfoViewModel CategoryInfo {
            get => _categoryInfo;
            set => this.RaiseAndSetIfChanged(ref _categoryInfo, value);
        }

        public ObservableCollection<BaseViewModel> Criterions {
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
    }
}