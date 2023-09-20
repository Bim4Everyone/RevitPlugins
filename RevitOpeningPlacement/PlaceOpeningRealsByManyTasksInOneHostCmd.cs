using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.RealOpeningPlacement;

namespace RevitOpeningPlacement {
    /// <summary>
    /// Команда для размещения чистовых отверстий АР в одной выбранной конструкции в местах пересечения с выбранными заданиями на отверстия.
    /// При этом для каждого задания создается отдельное чистовое отверстие, то есть объединения не происходит.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class PlaceOpeningRealsByManyTasksInOneHostCmd : OpeningRealPlacerCmd {
        public PlaceOpeningRealsByManyTasksInOneHostCmd() : base("Принять несколько заданий без объединения") { }


        public void ExecuteCommand(UIApplication uiApplication) {
            Execute(uiApplication);
        }


        protected override void Execute(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            if(!ModelCorrect(revitRepository)) {
                return;
            }
            var placer = new RealOpeningPlacer(revitRepository);
            placer.PlaceSinglesByManyTasks();
        }
    }
}
