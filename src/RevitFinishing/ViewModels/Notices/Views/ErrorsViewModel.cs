using dosymep.SimpleServices;

namespace RevitFinishing.ViewModels.Notices;
internal class ErrorsViewModel : NoticeViewModel {
    public ErrorsViewModel(ILocalizationService localizationService)
        : base(localizationService) {
        NoticeInfo = _localizationService.GetLocalizedString("ErrorsWindow.ErrorsDescription");
    }
}
