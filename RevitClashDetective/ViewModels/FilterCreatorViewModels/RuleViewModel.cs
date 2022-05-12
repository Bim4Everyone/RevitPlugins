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
        private ParamValue _selectedValue;
        private ObservableCollection<RuleEvaluatorViewModel> _ruleEvaluators;
        private RuleEvaluatorViewModel _selectedRuleEvaluator;
        private ObservableCollection<ParamValue> _values;

        public RuleViewModel(RevitRepository revitRepository, CategoriesInfoViewModel categoriesInfo) {
            _revitRepository = revitRepository;
            CategoriesInfo = categoriesInfo;

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

        public ObservableCollection<ParamValue> Values {
            get => _values;
            set => this.RaiseAndSetIfChanged(ref _values, value);
        }

        public ParamValue SelectedValue {
            get => _selectedValue;
            set => this.RaiseAndSetIfChanged(ref _selectedValue, value);
        }

        public void Renew() {
            SelectedParameter = null;
            SelectedRuleEvaluator = null;

        }

        private void ParameterSelectionChanged(object p) {
            if(SelectedParameter != null) {
                RuleEvaluators = new ObservableCollection<RuleEvaluatorViewModel>(SelectedParameter.GetEvaluators().Select(item => new RuleEvaluatorViewModel(item)));
                SelectedRuleEvaluator = RuleEvaluators.FirstOrDefault();
                if(SelectedRuleEvaluator != null) {
                    EvaluatorSelectionChanged(null);
                }
            }


        }

        private void EvaluatorSelectionChanged(object p) {
            if(SelectedParameter != null && SelectedRuleEvaluator != null) {
                Values = new ObservableCollection<ParamValue>(SelectedParameter.GetValues(CategoriesInfo.Categories.Select(item => item.Category), SelectedRuleEvaluator.RuleEvaluator));
            } else {
                Values = new ObservableCollection<ParamValue>();
            }
        }
    }
}
