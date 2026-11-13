using System.Collections.Generic;
using System.Linq;

using Ninject;
using Ninject.Syntax;

using RevitDeclarations.ViewModels;
using RevitDeclarations.Views;

namespace RevitDeclarations.Services;

internal class ErrorWindowService {
    private readonly IResolutionRoot _resolutionRoot;

    public ErrorWindowService(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot;
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
