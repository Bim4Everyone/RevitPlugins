using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.FilterableValueProviders;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class CategoriesInfoViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<ParameterViewModel> _parameters;

        public CategoriesInfoViewModel(RevitRepository revitRepository, IEnumerable<Category> categories) {
            _revitRepository = revitRepository;
            Categories = new ObservableCollection<Category>(categories);
            InitializeParameters();
        }

        public ObservableCollection<Category> Categories {
            get => _categories;
            set => RaiseAndSetIfChanged(ref _categories, value);
        }


        public ObservableCollection<ParameterViewModel> Parameters {
            get => _parameters;
            set => RaiseAndSetIfChanged(ref _parameters, value);
        }

        public void InitializeParameters() {
            var parameters =
                _revitRepository.DocInfos
                .GroupBy(item => item.Name)
                .Select(group => group.ToList().First())
                .SelectMany(item => _revitRepository.GetParameters(item.Doc, Categories))
                .Distinct()
                .Select(item => new ParameterViewModel(item))
                .ToList();
            parameters.Add(new ParameterViewModel(new WorksetValueProvider(_revitRepository.GetClashRevitRepository())));
            Parameters = new ObservableCollection<ParameterViewModel>(parameters.OrderBy(item => item.Name));
        }
    }
}
