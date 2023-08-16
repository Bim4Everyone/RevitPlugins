using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.RealOpeningPlacement;

namespace RevitOpeningPlacement {
    /// <summary>
    /// Команда для размещения чистовых отверстий в АР/КР по полученным заданиям на отверстия из связей
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class PlaceOpeningRealCommand : BasePluginCommand {
        protected override void Execute(UIApplication uiApplication) {
            ExecuteCommand(uiApplication);
        }

        public void ExecuteCommand(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            var placer = new RealOpeningPlacer(revitRepository);
            placer.Place();
        }
    }
}
