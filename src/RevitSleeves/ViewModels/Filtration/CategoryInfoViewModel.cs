
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.FilterableValueProviders;

using RevitSleeves.Models;

namespace RevitSleeves.ViewModels.Filtration;
internal class CategoryInfoViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    public CategoryInfoViewModel(
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        Category category) {

        _revitRepository = revitRepository
            ?? throw new System.ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService
            ?? throw new System.ArgumentNullException(nameof(localizationService));
        Category = category ?? throw new System.ArgumentNullException(nameof(category));
        Name = Category.Name;
        Parameters = InitializeParameters(_localizationService, revitRepository, category);
    }

    public string Name { get; }

    public Category Category { get; }

    public IReadOnlyCollection<ParameterViewModel> Parameters { get; }


    private IReadOnlyCollection<ParameterViewModel> InitializeParameters(
        ILocalizationService localizationService,
        RevitRepository repository,
        Category category) {

        var parameters = _revitRepository.GetParameters(_revitRepository.Document, category)
            .Distinct()
            .Select(item => new ParameterViewModel(localizationService, item))
            .ToList();
        parameters.Add(new ParameterViewModel(localizationService,
            new WorksetValueProvider(repository.GetClashRevitRepository())));
        return new ObservableCollection<ParameterViewModel>(parameters.OrderBy(item => item.Name));
    }
}
