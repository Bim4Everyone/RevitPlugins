using System;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Models.Filtration;

namespace RevitSleeves.ViewModels.Settings;
internal class StructureCategoryViewModel : BaseViewModel {
    private bool _isEnabled;

    public StructureCategoryViewModel(
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        ILanguageService languageService,
        ILogicalFilterProviderFactory filterProviderFactory,
        IFilterContextParser filterContextParser,
        StructureSettings structureSettings) {

        if(revitRepository is null) {
            throw new ArgumentNullException(nameof(revitRepository));
        }
        if(filterProviderFactory is null) {
            throw new ArgumentNullException(nameof(filterProviderFactory));
        }
        if(structureSettings is null) {
            throw new ArgumentNullException(nameof(structureSettings));
        }
        LanguageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        LocalizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        FilterContextParser = filterContextParser ?? throw new ArgumentNullException(nameof(filterContextParser));

        InitializeCategory(revitRepository, filterProviderFactory, structureSettings);
    }


    public string Name { get; private set; }

    public ILogicalFilterProvider FilterProvider { get; private set; }

    public ILanguageService LanguageService { get; }

    public ILocalizationService LocalizationService { get; }

    public IFilterContextParser FilterContextParser { get; }

    public bool IsEnabled {
        get => _isEnabled;
        set => RaiseAndSetIfChanged(ref _isEnabled, value);
    }


    private void InitializeCategory(RevitRepository revitRepository,
        ILogicalFilterProviderFactory filterProviderFactory,
        StructureSettings structureSettings) {
        var category = revitRepository.GetCategory(structureSettings.Category);

        var dataProvider = new FilterDataProvider(category, [revitRepository.Document]).CreateDataProvider();
        FilterProvider = FilterContextParser.TryParse(structureSettings.FilterContext, out var context)
            ? filterProviderFactory.Create(dataProvider, context!)
            : filterProviderFactory.Create(dataProvider);
        Name = category.Name;
        IsEnabled = structureSettings.IsEnabled;
    }

    public T GetStructureSettings<T>() where T : StructureSettings, new() {
        return new T() {
            FilterContext = FilterContextParser.Serialize(FilterProvider.GetFilter()),
            IsEnabled = IsEnabled
        };
    }
}
