using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ninject;
using Ninject.Syntax;

using RevitRooms.ViewModels;
using RevitRooms.Views;

namespace RevitRooms.Services;

internal class ErrorWindowService {
    private readonly IResolutionRoot _resolutionRoot;

    public ErrorWindowService(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot;
    }

    public bool ShowNoticeWindow(string title, 
                                 bool notShowWarnings,
                                 IEnumerable<WarningViewModel> warnings) {
        if(notShowWarnings) {
            warnings = warnings
                .Where(item => item.TypeInfo != TypeInfo.Warning)
                .OrderBy(x => x.TypeInfo);
        }

        if(warnings.Any()) {
            var window = _resolutionRoot.Get<InfoElementsWindow>();
            window.Title = title;
            window.DataContext = new WarningsViewModel() {
                Warnings = [.. warnings.OrderBy(x => x.TypeInfo)]
            };

            window.Show();
            return true;
        }

        return false;
    }
}
