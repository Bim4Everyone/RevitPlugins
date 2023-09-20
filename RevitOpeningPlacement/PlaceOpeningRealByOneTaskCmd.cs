using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.RealOpeningPlacement;

namespace RevitOpeningPlacement {
    /// <summary>
    /// Команда для размещения одного чистового отверстия в АР/КР по одному полученному заданию на отверстия из связи ВИС
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class PlaceOpeningRealByOneTaskCmd : OpeningRealPlacerCmd {
        public PlaceOpeningRealByOneTaskCmd() : base("Принять одно задание на отверстие") { }


        public void ExecuteCommand(UIApplication uiApplication) {
            ExecuteCommand(uiApplication);
        }


        protected override void Execute(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            if(!ModelCorrect(revitRepository)) {
                return;
            }
            var placer = new RealOpeningPlacer(revitRepository);
            placer.PlaceBySingleTask();
        }
    }
}
