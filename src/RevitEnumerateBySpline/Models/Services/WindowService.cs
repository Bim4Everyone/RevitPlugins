using System.Collections.Generic;

using Ninject;
using Ninject.Syntax;

using RevitEnumerateBySpline.Models.Warnings;
using RevitEnumerateBySpline.ViewModels.Warnings;
using RevitEnumerateBySpline.Views;

namespace RevitEnumerateBySpline.Models.Services;

internal class WindowService(IResolutionRoot resolutionRoot) {
    private MainWindow? _mainWindow;
    private IReadOnlyCollection<WarningElement> _pendingWarnings = [];

    public void SetMainWindow(MainWindow mainWindow) {
        _mainWindow = mainWindow;
    }

    public void RequestCurveSelection() {
        _mainWindow!.RequestCurveSelection();
    }

    public void RequestWarnings(IReadOnlyCollection<WarningElement> warnings) {
        _pendingWarnings = warnings;
        _mainWindow!.RequestWarnings();
    }

    public void ShowWarnings() {
        var viewModel = resolutionRoot.Get<WarningsViewModel>();
        viewModel.LoadView(_pendingWarnings);
        var window = resolutionRoot.Get<WarningsWindow>();
        window.DataContext = viewModel;
        _pendingWarnings = [];
        window.ShowDialog();
    }
}
