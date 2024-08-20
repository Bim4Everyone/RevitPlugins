using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
            var parameters =
                _revitRepository.DocInfos
                .SelectMany(item => _revitRepository.GetParameters(item.Doc, Categories.Select(c => c.Category)))
                .Distinct()
                .Select(item => new ParameterViewModel(item))
                .ToList();
            parameters.Add(new ParameterViewModel(new WorksetValueProvider(_revitRepository)));
            Parameters = new ObservableCollection<ParameterViewModel>(parameters.OrderBy(item => item.Name));
        }
    }
}
