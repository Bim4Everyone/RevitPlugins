using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.FilterableValueProviders;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
internal class CategoriesInfoViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;
    private ObservableCollection<Category> _categories;
    private ObservableCollection<ParameterViewModel> _parameters;

    public CategoriesInfoViewModel(RevitRepository revitRepository,
        ILocalizationService localization,
        IEnumerable<Category> categories) {
        _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new System.ArgumentNullException(nameof(localization));
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
        var parameters = _revitRepository.GetParameters(_revitRepository.Doc, Categories)
            .Distinct()
            .Select(item => new ParameterViewModel(_localization, item))
            .ToList();
        parameters.Add(new ParameterViewModel(_localization,
            new WorksetValueProvider(_revitRepository.GetClashRevitRepository())));
        Parameters = new ObservableCollection<ParameterViewModel>(parameters.OrderBy(item => item.Name));
    }
}
