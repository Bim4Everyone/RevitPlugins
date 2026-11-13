using Bim4Everyone.RevitFiltration.Controls;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitMarkAllDocuments.ViewModels;

internal class FilterPageViewModel : BaseViewModel {
    public FilterPageViewModel(ILogicalFilterProviderFactory filterProviderFactory,
                               DataProvider dataProvider,
                               ILocalizationService languageService) {
        FilterProvider = filterProviderFactory.Create(dataProvider);
        LanguageService = languageService;
    }

    public FilterPageViewModel(ILogicalFilterProviderFactory filterProviderFactory,
                               DataProvider dataProvider,
                               ILogicalFilterContext context,
                               ILocalizationService languageService) {
        FilterProvider = filterProviderFactory.Create(dataProvider, context);
        LanguageService = languageService;
    }

    public ILogicalFilterProvider FilterProvider { get; }
    public ILocalizationService LanguageService { get; }
}
