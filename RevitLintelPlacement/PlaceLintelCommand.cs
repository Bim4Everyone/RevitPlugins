using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitLintelPlacement.ViewModels;
using RevitLintelPlacement.Views;

namespace RevitLintelPlacement {

    [Transaction(TransactionMode.Manual)]
    public class PlaceLintelCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            var mainViewModel = new MainViewModel();
            var view = new MainWindow() { DataContext = mainViewModel };
            view.ShowDialog();
            return Result.Succeeded;
        }
    }
}
