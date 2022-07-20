using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.OpeningPlacement;

namespace RevitOpeningPlacement {

    [Transaction(TransactionMode.Manual)]
    public class PlaceOpeningTaskCommand : BasePluginCommand {
        public PlaceOpeningTaskCommand() {
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
                InitializeProgress(revitRepository, placers);
            }
        }

        private void InitializeProgress(RevitRepository revitRepository, IEnumerable<OpeningPlacer> placers) {
            using(var pb = GetPlatformService<IProgressDialogService>()) {
                pb.StepValue = 10;
                pb.DisplayTitleFormat = "Идёт расчёт... [{0}\\{1}]";
                var progress = pb.CreateProgress();
                pb.MaxValue = placers.Count();
                var ct = pb.CreateCancellationToken();
                pb.Show();

                PlaceOpenings(progress, ct, revitRepository, placers);
            }
        }

        private void PlaceOpenings(IProgress<int> progress, CancellationToken ct, RevitRepository revitRepository, IEnumerable<OpeningPlacer> placers) {
            using(var t = revitRepository.GetTransaction("Расстановка заданий.")) {
                int count = 0;
                foreach(var p in placers) {
                    p.Place();
                    progress.Report(count++);
                }
                t.Commit();
            }
        }
    }
}
