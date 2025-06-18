using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.ViewModels.Filtration;

namespace RevitSleeves.ViewModels.Settings;
internal class MepCategoryViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly MepCategorySettings _mepCategorySettings;

    public MepCategoryViewModel(RevitRepository revitRepository,
        ILocalizationService localizationService,
        MepCategorySettings mepCategorySettings) {

        _revitRepository = revitRepository
            ?? throw new System.ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService
            ?? throw new System.ArgumentNullException(nameof(localizationService));
        _mepCategorySettings = mepCategorySettings
            ?? throw new System.ArgumentNullException(nameof(mepCategorySettings));

        InitializeCategory(_revitRepository, _localizationService, _mepCategorySettings);
    }


    public SetViewModel MepFilterViewModel { get; private set; }


    private void InitializeCategory(RevitRepository revitRepository,
        ILocalizationService localizationService,
        MepCategorySettings mepCategorySettings) {

        MepFilterViewModel = new SetViewModel(
            revitRepository,
            localizationService,
            new CategoryInfoViewModel(
                revitRepository,
                localizationService,
                revitRepository.GetCategory(mepCategorySettings.Category)));
    }
}
