using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterableValueProviders;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels;
internal class CategoriesInfoViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;
    private ObservableCollection<CategoryViewModel> _categories;
    private ObservableCollection<ParameterViewModel> _parameters;

    public CategoriesInfoViewModel(RevitRepository revitRepository,
        ILocalizationService localization,
        IEnumerable<CategoryViewModel> categories) {
        _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new System.ArgumentNullException(nameof(localization));
        Categories = new ObservableCollection<CategoryViewModel>(categories);
        InitializeParameters();
    }

    public ObservableCollection<CategoryViewModel> Categories {
        get => _categories;
        set => RaiseAndSetIfChanged(ref _categories, value);
    }


    public ObservableCollection<ParameterViewModel> Parameters {
        get => _parameters;
        set => RaiseAndSetIfChanged(ref _parameters, value);
    }

    public void InitializeParameters() {
        var parameters = _revitRepository.GetParameters(_revitRepository.Doc, Categories.Select(c => c.Category))
            .Distinct()
            .Select(item => new ParameterViewModel(_localization, item))
            .ToList();
        parameters.Add(new ParameterViewModel(_localization, new WorksetValueProvider(_revitRepository)));
        Parameters = new ObservableCollection<ParameterViewModel>(parameters.OrderBy(item => item.Name));
    }
}
