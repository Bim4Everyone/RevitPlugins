using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.ViewModels.FilterCreatorViewModels.Interfaces;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class RuleViewModel : BaseViewModel, IСriterionViewModel {
        private readonly RevitRepository _revitRepository;
        private CategoriesInfoViewModel categoriesInfo;
        private ParameterViewModel _selectedParmaeter;

        public RuleViewModel(RevitRepository revitRepository, CategoriesInfoViewModel categoriesInfo) {
            _revitRepository = revitRepository;
            CategoriesInfo = categoriesInfo;

            SelectionChangedCommand = new RelayCommand(SelectionChanged);
        }

        public ICommand SelectionChangedCommand { get; set; }

        public ParameterViewModel SelectedParameter {
            get => _selectedParmaeter;
            set => this.RaiseAndSetIfChanged(ref _selectedParmaeter, value);
        }

        public CategoriesInfoViewModel CategoriesInfo {
            get => categoriesInfo;
            set => this.RaiseAndSetIfChanged(ref categoriesInfo, value);
        }

        private void SelectionChanged(object p) {
            if(SelectedParameter == null) {
                SelectedParameter = CategoriesInfo.Parameters.FirstOrDefault();
            }
        }
    }
}
