using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class GroupedRuleViewModel : BaseViewModel {
        private string _name;
        private ObservableCollection<ConcreteRuleViewModel> _gruopedRules;
        private WallTypeConditionViewModel _groupingCondition;

        public GroupedRuleViewModel() {
            AddRuleCommand = new RelayCommand(AddRule, p => true);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public WallTypeConditionViewModel GroupingCondition {
            get => _groupingCondition;
            set => this.RaiseAndSetIfChanged(ref _groupingCondition, value);
        }

        public ObservableCollection<ConcreteRuleViewModel> Rules {
            get => _gruopedRules;
            set => this.RaiseAndSetIfChanged(ref _gruopedRules, value);
        }

        public ICommand AddRuleCommand { get; set; }

        private void AddRule(object p) {
            Rules.Add(new ConcreteRuleViewModel());
        }

    }

    internal class ConcreteRuleViewModel : BaseViewModel {
        private string _selectedLintelType;
        private OpeningWidthConditionViewModel _openingWidthCondition;
        private NumberLintelParameterViewModel _offsetParameterViewModel;
        private List<string> _lintelTypes;

        public string SelectedLintelType { 
            get => _selectedLintelType; 
            set => this.RaiseAndSetIfChanged(ref _selectedLintelType, value); 
        }


        public OpeningWidthConditionViewModel OpeningWidthCondition {
            get => _openingWidthCondition;
            set => this.RaiseAndSetIfChanged(ref _openingWidthCondition, value);
        }
        public NumberLintelParameterViewModel OffsetParameterViewModel {
            get => _offsetParameterViewModel;
            set => this.RaiseAndSetIfChanged(ref _offsetParameterViewModel, value);
        }

        public List<string> LintelTypes {
            get => _lintelTypes;
            set => this.RaiseAndSetIfChanged(ref _lintelTypes, value);
        }

       
    }
}
