
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers;
using RevitOpeningPlacement.Models.OpeningUnion;
using RevitOpeningPlacement.Models.OpeningUnion.IntersectionProviders;

namespace RevitOpeningPlacement {
    [Transaction(TransactionMode.Manual)]
    public class UniteOpeningTasksCommand : BasePluginCommand {
        protected override void Execute(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            var configurator = new UnionGroupsConfigurator(revitRepository);
            var placers = GetPlacersForUnion(revitRepository, configurator);
            UniteOpeningsGroups(revitRepository, placers);
            revitRepository.DeleteElements(configurator.GetElementsToDelete());
        }

        private void UniteOpeningsGroups(RevitRepository revitRepository, List<OpeningPlacer> placers) {
            using(var pb = GetPlatformService<IProgressDialogService>()) {
                IProgress<int> progress;
                CancellationToken ct;
                InitializeProgress(placers.Count, pb, out progress, out ct);

                UniteOpeningsGroups(progress, ct, revitRepository, placers);
            }
        }

        private List<OpeningPlacer> GetPlacersForUnion(RevitRepository revitRepository, UnionGroupsConfigurator configurator) {
            var placers = new List<OpeningPlacer>();
            using(var pb = GetPlatformService<IProgressDialogService>()) {
                IProgress<int> progress;
                CancellationToken ct;
                var maxValue = revitRepository.GetOpeningsTaskFromCurrentDoc().Count;
                InitializeProgress(maxValue, pb, out progress, out ct);

                placers.AddRange(configurator.GetPlacers(progress, ct));
            }
            return placers;
        }

        private void UniteOpeningsGroups(IProgress<int> progress, CancellationToken ct, RevitRepository revitRepository, List<OpeningPlacer> placers) {
            using(var t = revitRepository.GetTransaction("Объединение заданий")) {
                int count = 0;
                foreach(var placer in placers) {
                    placer.Place();
                    progress.Report(count++);
                    ct.ThrowIfCancellationRequested();
                }
                t.Commit();
            }
        }

        private static void InitializeProgress(int maxValue, IProgressDialogService pb, out IProgress<int> progress, out CancellationToken ct) {
            pb.StepValue = 10;
            pb.DisplayTitleFormat = "Идёт расчёт... [{0}\\{1}]";
            progress = pb.CreateProgress();
            pb.MaxValue = maxValue;
            ct = pb.CreateCancellationToken();
            pb.Show();
        }
    }
}