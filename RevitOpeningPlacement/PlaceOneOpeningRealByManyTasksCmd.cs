using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.RealOpeningArPlacement;

namespace RevitOpeningPlacement {
    /// <summary>
    /// Команда для размещения одного чистового отверстия в АР/КР по одному или нескольким полученным заданиям на отверстия из связей ВИС.
    /// При этом будет создано одно чистовое отверстие, которое объединит все задания на отверстия.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class PlaceOneOpeningRealByManyTasksCmd : OpeningRealPlacerCmd {
        public PlaceOneOpeningRealByManyTasksCmd() : base("Принять несколько заданий с объединением") { }


        public void ExecuteCommand(UIApplication uiApplication) {
            Execute(uiApplication);
        }


        protected override void Execute(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            if(!ModelCorrect(revitRepository)) {
                return;
            }
            var placer = new RealOpeningArPlacer(revitRepository);
            placer.PlaceUnitedOpeningByManyTasks();
        }
    }
}
