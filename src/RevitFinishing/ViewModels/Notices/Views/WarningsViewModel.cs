using dosymep.SimpleServices;

namespace RevitFinishing.ViewModels.Notices;
internal class WarningsViewModel : NoticeViewModel {
    public WarningsViewModel(ILocalizationService localizationService)
        : base(localizationService) {
        NoticeInfo = _localizationService.GetLocalizedString("ErrorsWindow.WarningsDescription");
    }
}
