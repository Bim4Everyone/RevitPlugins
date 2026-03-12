using System.Collections.Generic;

using Ninject;
using Ninject.Syntax;

using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.ViewModels;
using RevitBuildCoordVolumes.Views;

namespace RevitBuildCoordVolumes.Models.Services;

internal class WindowService : IWindowService {
    private readonly IResolutionRoot _resolutionRoot;

    public WindowService(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot;
    }

    public void CloseMainWindow() {
        var mainWindow = _resolutionRoot.Get<MainWindow>();
        mainWindow.Close();
    }

    public void ShowWarningWindow(IReadOnlyCollection<WarningElement> warningElements) {
        var warningsVM = _resolutionRoot.Get<WarningsViewModel>();
        warningsVM.WarningElementsCollection = warningElements;
        var warningsWindow = _resolutionRoot.Get<WarningsWindow>();
        warningsWindow.DataContext = warningsVM;
        warningsVM.LoadView();
        warningsWindow.Show();
    }
}
