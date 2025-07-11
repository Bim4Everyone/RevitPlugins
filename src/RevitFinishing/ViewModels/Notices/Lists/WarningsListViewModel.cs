using dosymep.SimpleServices;

namespace RevitFinishing.ViewModels.Notices;
internal class WarningsListViewModel : NoticeListViewModel {
    public WarningsListViewModel(ILocalizationService localizationService) {
        Status = localizationService.GetLocalizedString("ErrorsWindow.ListWarningType");
    }
}
