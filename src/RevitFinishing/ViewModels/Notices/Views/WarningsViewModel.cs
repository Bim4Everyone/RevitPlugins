using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.SimpleServices;

namespace RevitFinishing.ViewModels.Errors
{
    internal class WarningsViewModel : NoticeViewModel {
        public WarningsViewModel(ILocalizationService localizationService)
            : base(localizationService) {
            NoticeInfo = _localizationService.GetLocalizedString("ErrorsWindow.WarningsDescription");
        }
    }
}
