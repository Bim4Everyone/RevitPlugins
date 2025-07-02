using dosymep.SimpleServices;

namespace RevitFinishing.ViewModels.Notices;
internal class WarningsViewModel : NoticeViewModel {
    public WarningsViewModel(ILocalizationService localizationService)
        : base(localizationService) {
        NoticeInfoTitle = _localizationService.GetLocalizedString("ErrorsWindow.WarningInfoTitle");
        NoticeInfo = _localizationService.GetLocalizedString("ErrorsWindow.WarningInfo");
    }
}
