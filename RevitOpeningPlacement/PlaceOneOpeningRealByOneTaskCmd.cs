using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.RealOpeningArPlacement;

namespace RevitOpeningPlacement {
    /// <summary>
    /// Команда для размещения одного чистового отверстия в АР/КР по одному полученному заданию на отверстия из связи ВИС
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class PlaceOneOpeningRealByOneTaskCmd : OpeningRealPlacerCmd {
        public PlaceOneOpeningRealByOneTaskCmd() : base("Принять одно задание на отверстие") { }


        public void ExecuteCommand(UIApplication uiApplication) {
            Execute(uiApplication);
        }


        protected override void Execute(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            if(!ModelCorrect(revitRepository)) {
                return;
            }
            var placer = new RealOpeningArPlacer(revitRepository);
            placer.PlaceSingleOpeningByOneTask();
        }
    }
}
