using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterableValueProviders;

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
            var parameterInitializer = new ParameterInitializer(_revitRepository);
            Parameters = new ObservableCollection<ParameterViewModel>(
                _revitRepository.GetDocuments()
                .SelectMany(item => _revitRepository.GetParameters(item, Categories.Select(c => c.Category))
                    .Select(p => new ParameterValueProvider(_revitRepository) { RevitParam = parameterInitializer.InitializeParameter(item, p) }))
                .Distinct()
                .Select(item=>new ParameterViewModel(item))
                .OrderBy(item=>item.Name));
        }
    }
}
