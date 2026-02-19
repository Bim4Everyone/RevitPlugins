using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.SimpleServices;

using Ninject;
using Ninject.Syntax;

using RevitDeclarations.ViewModels;
using RevitDeclarations.Views;

namespace RevitDeclarations.Services;

internal class ErrorWindowService {
    private readonly IResolutionRoot _resolutionRoot;
    private readonly ILocalizationService _localizationService;

    public ErrorWindowService(IResolutionRoot resolutionRoot, ILocalizationService localizationService) {
        _resolutionRoot = resolutionRoot;
        _localizationService = localizationService;
    }

    public bool ShowNoticeWindow(IList<WarningViewModel> allWarnings, bool isWarning = false) {
        if(allWarnings.Any()) {
            var window = _resolutionRoot.Get<WarningsWindow>();
            window.DataContext = new WarningsViewModel() {
                Warnings = [.. allWarnings],
                IsWarning = isWarning
            };

            window.ShowDialog();
            if((bool) window.DialogResult) {
                return true;
            }
        }

        return false;
    }
}
