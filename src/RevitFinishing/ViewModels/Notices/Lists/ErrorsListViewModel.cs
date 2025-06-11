using System.Collections.ObjectModel;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitFinishing.ViewModels
{
    internal class ErrorsListViewModel : NoticeListViewModel {
        public ErrorsListViewModel(ILocalizationService localizationService) {
            Status = localizationService.GetLocalizedString("ErrorsWindow.ListErrorType");
        }
    }
}
