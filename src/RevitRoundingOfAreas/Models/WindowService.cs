using System.Collections.Generic;

using Ninject;
using Ninject.Syntax;

using RevitRoundingOfAreas.Models.Interfaces;
using RevitRoundingOfAreas.Models.Warnings;
using RevitRoundingOfAreas.ViewModels.Warnings;
using RevitRoundingOfAreas.Views;

namespace RevitRoundingOfAreas.Models;

internal class WindowService(IResolutionRoot resolutionRoot) : IWindowService {
    public void CloseMainWindow() {
        var mainWindow = resolutionRoot.Get<MainWindow>();
        mainWindow.Close();
    }

    public void ShowWarningWindow(IReadOnlyCollection<WarningElement> warningElements) {
        var warningViewModel = resolutionRoot.Get<WarningsViewModel>();
        warningViewModel.WarningElementsCollection = warningElements;
        var warningsWindow = resolutionRoot.Get<WarningsWindow>();
        warningsWindow.DataContext = warningViewModel;
        warningViewModel.LoadView();
        warningsWindow.Show();
    }
}
