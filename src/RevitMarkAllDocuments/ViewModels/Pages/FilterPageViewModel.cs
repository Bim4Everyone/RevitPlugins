using Bim4Everyone.RevitFiltration.Controls;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitMarkAllDocuments.ViewModels;

internal class FilterPageViewModel : BaseViewModel {
    public FilterPageViewModel(ILogicalFilterProviderFactory filterProviderFactory,
                               IDataProvider dataProvider,
                               ILocalizationService languageService) {
        FilterProvider = filterProviderFactory.Create(dataProvider);
        LanguageService = languageService;
    }

    public ILogicalFilterProvider FilterProvider { get; }
    public ILocalizationService LanguageService { get; }
}
