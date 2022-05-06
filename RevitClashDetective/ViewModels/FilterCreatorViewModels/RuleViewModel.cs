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
using RevitClashDetective.ViewModels.FilterCreatorViewModels.Interfaces;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class RuleViewModel : BaseViewModel, IСriterionViewModel {
        private readonly RevitRepository _revitRepository;
        private CategoriesInfoViewModel _categoriesInfo;
        private ParameterViewModel _selectedParameter;
        private ObservableCollection<RuleEvaluatorViewModel> _ruleEvaluators;
        private RuleEvaluatorViewModel _selectedRuleEvaluator;
        private ObservableCollection<object> _values;
        private object _selectedValue;

        public RuleViewModel(RevitRepository revitRepository, CategoriesInfoViewModel categoriesInfo) {
            _revitRepository = revitRepository;
            CategoriesInfo = categoriesInfo;
            SelectedParameter = CategoriesInfo.Parameters.FirstOrDefault();
            if(SelectedParameter != null) {
                RuleEvaluators = new ObservableCollection<RuleEvaluatorViewModel>(SelectedParameter.GetEvaluators().Select(item => new RuleEvaluatorViewModel(item)));
                SelectedRuleEvaluator = RuleEvaluators.FirstOrDefault();
                if(SelectedRuleEvaluator != null) {
                    EvaluatorSelectionChanged(null);
                }
            } else {
                RuleEvaluators = new ObservableCollection<RuleEvaluatorViewModel>();
                SelectedRuleEvaluator = null;
                Values = new ObservableCollection<object>();
            }

            ParameterSelectionChangedCommand = new RelayCommand(ParameterSelectionChanged);
            EvaluatorSelectionChangedCommand = new RelayCommand(EvaluatorSelectionChanged);
        }

        public ICommand ParameterSelectionChangedCommand { get; set; }
        public ICommand EvaluatorSelectionChangedCommand { get; set; }

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

        public ObservableCollection<object> Values {
            get => _values;
            set => this.RaiseAndSetIfChanged(ref _values, value);
        }

        public object SelectedValue { 
            get => _selectedValue; 
            set => this.RaiseAndSetIfChanged(ref _selectedValue, value); 
        }

        private void Renew() {

        }

        private void ParameterSelectionChanged(object p) {
            if(SelectedParameter == null) {
                SelectedParameter = CategoriesInfo.Parameters.FirstOrDefault();
            }
            RuleEvaluators = new ObservableCollection<RuleEvaluatorViewModel>(SelectedParameter.GetEvaluators().Select(item => new RuleEvaluatorViewModel(item)));
            SelectedRuleEvaluator = RuleEvaluators.FirstOrDefault();
            if(SelectedRuleEvaluator != null) {
                EvaluatorSelectionChanged(null);
            }

        }

        private void EvaluatorSelectionChanged(object p) {
            Values = new ObservableCollection<object>(SelectedParameter.GetValues(CategoriesInfo.Categories.Select(item => item.Category), SelectedRuleEvaluator.RuleEvaluator));
        }
    }
}
