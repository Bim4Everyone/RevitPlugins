using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.SimpleServices;

namespace RevitFinishing.ViewModels
{
    internal class WarningsListViewModel : NoticeListViewModel {
        public WarningsListViewModel(ILocalizationService localizationService) {
            Status = localizationService.GetLocalizedString("ErrorsWindow.ListWarningType");
        }
    }
}
