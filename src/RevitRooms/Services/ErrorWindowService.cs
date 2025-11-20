using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.SimpleServices;

using Ninject;
using Ninject.Syntax;

using RevitRooms.Models;
using RevitRooms.ViewModels;
using RevitRooms.Views;

namespace RevitRooms.Services;

internal class ErrorWindowService {
    private readonly IResolutionRoot _resolutionRoot;
    private readonly ILocalizationService _localizationService;

    public ErrorWindowService(IResolutionRoot resolutionRoot, ILocalizationService localizationService) {
        _resolutionRoot = resolutionRoot;
        _localizationService = localizationService;
    }

    public bool ShowNoticeWindow(bool notShowWarnings,
                                 IEnumerable<WarningViewModel> allWarnings) {
        if(notShowWarnings) {
            allWarnings = allWarnings
                .Where(item => item.TypeInfo != WarningTypeInfo.Warning)
                .OrderBy(x => x.TypeInfo);
        }

        if(allWarnings.Any()) {
            bool hasErrors = allWarnings
                .Where(item => item.TypeInfo == WarningTypeInfo.Error)
                .Any();
            bool hasWarnings = allWarnings
                .Where(item => item.TypeInfo == WarningTypeInfo.Warning)
                .Any();

            string message;
            if(hasErrors && hasWarnings) {
                message = _localizationService.GetLocalizedString("WarningsWindow.MainInfoErrorsAndWarnings");
            } else if(hasErrors) {
                message = _localizationService.GetLocalizedString("WarningsWindow.MainInfoErrors");
            } else {
                message = _localizationService.GetLocalizedString("WarningsWindow.MainInfoWarnings");
            }

            var window = _resolutionRoot.Get<WarningsWindow>();
            window.DataContext = new WarningsViewModel() {
                Description = message,
                Warnings = [.. allWarnings.OrderBy(x => x.TypeInfo)]
            };

            window.Show();
            return true;
        }

        return false;
    }
}
