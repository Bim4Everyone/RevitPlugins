using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.OpeningPlacement;

namespace RevitOpeningPlacement {

    [Transaction(TransactionMode.Manual)]
    public class RevitPlaceOpeningTaskCommand : BasePluginCommand {
        public RevitPlaceOpeningTaskCommand() {
            PluginName = "Настройка ";
        }

        protected override void Execute(UIApplication uiApplication) {
            RevitClashDetective.Models.RevitRepository clashRevitRepository
                = new RevitClashDetective.Models.RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            var openingConfig = OpeningConfig.GetOpeningConfig();
            if(openingConfig.Categories.Count > 0) {
                var placers = PlacementConfigurator.GetPlacers(revitRepository, clashRevitRepository, openingConfig.Categories)
                                                   .ToList();
                using(var t = revitRepository.GetTransaction("Расстановка заданий.")) {
                    foreach(var p in placers) {
                        p.Place();
                    }
                    t.Commit();
                }
            }
        }
    }
}
