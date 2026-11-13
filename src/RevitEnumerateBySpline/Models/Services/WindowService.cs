using System.Collections.Generic;

using Ninject;
using Ninject.Syntax;

using RevitEnumerateBySpline.Models.Interfaces;
using RevitEnumerateBySpline.Models.Warnings;
using RevitEnumerateBySpline.ViewModels.Warnings;
using RevitEnumerateBySpline.Views;

namespace RevitEnumerateBySpline.Models.Services;

internal class WindowService : IWindowService {
    private readonly MainWindow? _mainWindow;
    private IResolutionRoot _resolutionRoot;

    public WindowService(MainWindow? mainWindow, IResolutionRoot resolutionRoot) {
        _mainWindow = mainWindow;
        _resolutionRoot = resolutionRoot;
        
    }

    public void HideMainWindow() {
        _mainWindow?.Hide();
    }

    public void CloseMainWindow() {
        _mainWindow?.Close();
    }

    public void ShowMainWindow() {
        _mainWindow?.ShowDialog();
    }
    
    public void ShowWarningWindow(IReadOnlyCollection<WarningElement> warningElements) {
        var warningViewModel = _resolutionRoot.Get<WarningsViewModel>();
        var warningsWindow = _resolutionRoot.Get<WarningsWindow>();
        warningsWindow.DataContext = warningViewModel;
        warningViewModel.LoadView(warningElements);
        warningsWindow.Show();
    }
}
