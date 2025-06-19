using dosymep.SimpleServices;

namespace RevitFinishing.ViewModels.Notices;
internal class ErrorsListViewModel : NoticeListViewModel {
    public ErrorsListViewModel(ILocalizationService localizationService) {
        Status = localizationService.GetLocalizedString("ErrorsWindow.ListErrorType");
    }
}
