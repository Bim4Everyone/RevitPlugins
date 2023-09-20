using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.RealOpeningPlacement;

namespace RevitOpeningPlacement {
    /// <summary>
    /// Команда для размещения чистовых отверстий АР в выбранных конструкциях, которые пересекаются с выбранными заданиями на отверстия.
    /// При этом для каждого задания создается отдельное чистовое отверстие, то есть объединения не происходит.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class PlaceOpeningRealsByManyTasksInManyHostsCmd : OpeningRealPlacerCmd {
        public PlaceOpeningRealsByManyTasksInManyHostsCmd() : base("Авторазмещение отверстий по заданиям") { }


        public void ExecuteCommand(UIApplication uiApplication) {
            Execute(uiApplication);
        }


        protected override void Execute(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            if(!ModelCorrect(revitRepository)) {
                return;
            }
            var placer = new RealOpeningPlacer(revitRepository);
            placer.FindAndPlaceSingleOpenings();
        }
    }
}
