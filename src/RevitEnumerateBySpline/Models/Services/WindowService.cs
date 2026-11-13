using System;
using System.Collections.Generic;

using Ninject;
using Ninject.Syntax;

using RevitEnumerateBySpline.Models.Interfaces;
using RevitEnumerateBySpline.Models.Warnings;
using RevitEnumerateBySpline.ViewModels.Warnings;
using RevitEnumerateBySpline.Views;

namespace RevitEnumerateBySpline.Models.Services;

internal class WindowService : IWindowService {
    private readonly MainWindow _mainWindow;
    private readonly IResolutionRoot _resolutionRoot;
    private Action? _pendingSelection;

    public WindowService(MainWindow mainWindow, IResolutionRoot resolutionRoot) {
        _mainWindow = mainWindow;
        _resolutionRoot = resolutionRoot;
        
    }

    public void RequestSelection(Action select) {
        _pendingSelection = select;
        _mainWindow.Hide();
    }

    public void CloseMainWindow(bool dialogResult) {
        _mainWindow.DialogResult = dialogResult;
    }

    public bool? ShowMainWindow() {
        while(true) {
            bool? result = _mainWindow.ShowDialog();
            if(_pendingSelection is not { } select) {
                return result;
            }

            // Hide завершает текущий ShowDialog. Выбор и повторный показ выполняются
            // после выхода из него, без вложенных модальных циклов одного окна.
            _pendingSelection = null;
            try {
                select();
            } catch {
                _mainWindow.Close();
                throw;
            }
        }
    }
    
    public void ShowWarningWindow(IReadOnlyCollection<WarningElement> warningElements) {
        var warningViewModel = _resolutionRoot.Get<WarningsViewModel>();
        var warningsWindow = _resolutionRoot.Get<WarningsWindow>();
        warningsWindow.DataContext = warningViewModel;
        warningViewModel.LoadView(warningElements);
        warningsWindow.Show();
    }
}
