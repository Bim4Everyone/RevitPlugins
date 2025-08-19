using System;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.ViewModels.Filtration;

namespace RevitSleeves.ViewModels.Settings;
internal class StructureCategoryViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly StructureSettings _structureSettings;
    private bool _isEnabled;

    public StructureCategoryViewModel(
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        StructureSettings structureSettings) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _structureSettings = structureSettings ?? throw new ArgumentNullException(nameof(structureSettings));

        InitializeCategory(_revitRepository, _localizationService, structureSettings);
    }


    public string Name { get; private set; }

    public SetViewModel StructureFilterViewModel { get; private set; }

    public bool IsEnabled {
        get => _isEnabled;
        set => RaiseAndSetIfChanged(ref _isEnabled, value);
    }


    private void InitializeCategory(RevitRepository revitRepository,
        ILocalizationService localizationService,
        StructureSettings structureSettings) {
        var category = revitRepository.GetCategory(structureSettings.Category);

        StructureFilterViewModel = new SetViewModel(
            revitRepository,
            localizationService,
            new CategoryInfoViewModel(
                revitRepository,
                localizationService,
                category),
            structureSettings.FilterSet);
        Name = category.Name;
        IsEnabled = structureSettings.IsEnabled;
    }

    public T GetStructureSettings<T>() where T : StructureSettings, new() {
        return new T() {
            FilterSet = StructureFilterViewModel.GetSet(),
            IsEnabled = IsEnabled
        };
    }
}
