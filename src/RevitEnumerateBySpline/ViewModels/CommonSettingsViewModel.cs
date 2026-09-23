using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models;
using RevitEnumerateBySpline.Models.Factories;

namespace RevitEnumerateBySpline.ViewModels;

internal class CommonSettingsViewModel(
    ILocalizationService localizationService,
    ProvidersFactory providersFactory,
    RevitRepository revitRepository)
    : BaseViewModel {
    
    public RangeViewModel? RangeViewModel {
        get ;
        private set => RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Метод загрузки окна
    /// </summary>
    public void LoadView() {
        RangeViewModel = new RangeViewModel(localizationService, providersFactory, revitRepository);
        RangeViewModel?.LoadView();
    }
}
