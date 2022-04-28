using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class CategoriesInfoViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private ObservableCollection<CategoryViewModel> _categories;
        private ObservableCollection<ParameterViewModel> _parameters;

        public CategoriesInfoViewModel(RevitRepository revitRepository, IEnumerable<CategoryViewModel> categories) {
            _revitRepository = revitRepository;
            Categories = new ObservableCollection<CategoryViewModel>(categories);
            InitializeParameters();
        }

        public ObservableCollection<CategoryViewModel> Categories {
            get => _categories;
            set => this.RaiseAndSetIfChanged(ref _categories, value);
        }


        public ObservableCollection<ParameterViewModel> Parameters { 
            get => _parameters; 
            set => this.RaiseAndSetIfChanged(ref _parameters, value); 
        }

        public void InitializeParameters() {
            Parameters = new ObservableCollection<ParameterViewModel>(
                _revitRepository.GetParameters(Categories.Select(item=>item.Category))
                .OrderBy(item=>item.Name)
                .Select(item => new ParameterViewModel(item.Name)));
        }
    }
}
