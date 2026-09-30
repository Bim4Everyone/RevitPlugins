using RevitEnumerateBySpline.Views;

namespace RevitEnumerateBySpline.Models.Services;

internal class WindowService  {
    private readonly MainWindow _mainWindow;

    public WindowService(MainWindow mainWindow) {
        _mainWindow = mainWindow;
    }

    public void HideMainWindow() {
        _mainWindow.Hide();
    }
    
    public void CloseMainWindow() {
        _mainWindow.Close();
    }

    public void ShowMainWindow() {
        _mainWindow.ShowDialog();
    }
}
