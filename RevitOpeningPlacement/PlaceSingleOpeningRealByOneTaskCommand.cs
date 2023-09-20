using System;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.RealOpeningPlacement;
using RevitOpeningPlacement.Models.RealOpeningPlacement.Checkers;

namespace RevitOpeningPlacement {
    /// <summary>
    /// Команда для размещения одного чистового отверстия в АР/КР по одному полученному заданию на отверстия из связи ВИС
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class PlaceSingleOpeningRealByOneTaskCommand : BasePluginCommand {
        public PlaceSingleOpeningRealByOneTaskCommand() {
            PluginName = "Размещение чистового отверстия";
        }


        protected override void Execute(UIApplication uiApplication) {
            ExecuteCommand(uiApplication);
        }

        public void ExecuteCommand(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            if(!ModelCorrect(revitRepository)) {
                return;
            }
            var placer = new RealOpeningPlacer(revitRepository);
            placer.PlaceBySingleTask();
        }


        private bool ModelCorrect(RevitRepository revitRepository) {
            var checker = new RealOpeningsChecker(revitRepository);
            var errors = checker.GetErrorTexts();
            if(errors == null || errors.Count == 0) {
                return true;
            }

            TaskDialog.Show("BIM", $"{string.Join($"{Environment.NewLine}", errors)}");
            return false;
        }
    }
}
