using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.FilterModel;

using RevitSleeves.Models;

namespace RevitSleeves.ViewModels.Settings;

internal class FilterCheckerViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    public FilterCheckerViewModel(RevitRepository revitRepository,
        ILocalizationService localizationService,
        Filter filter) {

        _revitRepository = revitRepository
            ?? throw new System.ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService
            ?? throw new System.ArgumentNullException(nameof(localizationService));
        Filter = filter
            ?? throw new System.ArgumentNullException(nameof(filter));
    }

    public Filter Filter { get; }
}
