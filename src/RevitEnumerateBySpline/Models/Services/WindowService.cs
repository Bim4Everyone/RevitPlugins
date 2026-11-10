using RevitEnumerateBySpline.Views;

namespace RevitEnumerateBySpline.Models.Services;

internal class WindowService  {
    private MainWindow _mainWindow;

    public WindowService(MainWindow mainWindow) {
        _mainWindow = mainWindow;
    }

    public void SetMainWindow(MainWindow mainWindow) {
        _mainWindow = mainWindow;
    }

    public void RequestCurveSelection() {
        _mainWindow.RequestCurveSelection();
    }
}
