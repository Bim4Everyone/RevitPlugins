using dosymep.SimpleServices;

namespace RevitFinishing.ViewModels.Notices;
internal class ErrorsViewModel : NoticeViewModel {
    public ErrorsViewModel(ILocalizationService localizationService)
        : base(localizationService) {
        NoticeInfoTitle = _localizationService.GetLocalizedString("ErrorsWindow.ErrorInfoTitle");
        NoticeInfo = _localizationService.GetLocalizedString("ErrorsWindow.ErrorInfo");
    }
}
