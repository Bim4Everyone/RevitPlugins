using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.Checkers;
using RevitOpeningPlacement.ViewModels.ReportViewModel;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement {

    [Transaction(TransactionMode.Manual)]
    public class PlaceOpeningTaskCommand : BasePluginCommand {
        public PlaceOpeningTaskCommand() {
            PluginName = "Расстановка заданий на отверстия";
        }

        protected override void Execute(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            if(!CheckModel(revitRepository)) {
                return;
            }
            revitRepository.DeleteAllOpenings();
            var openingConfig = OpeningConfig.GetOpeningConfig();
            if(openingConfig.Categories.Count > 0) {
                var placementConfigurator = new PlacementConfigurator(revitRepository, openingConfig.Categories);
                var placers = placementConfigurator.GetPlacers()
                                                   .ToList();
                InitializePlacing(revitRepository, placers);
                InitializeReport(revitRepository, placementConfigurator.GetUnplacedClashes());
            }
        }

        private bool CheckModel(RevitRepository revitRepository) {
            var checker = new Checkers(revitRepository);
            var errors = checker.GetErrorTexts();
            if(errors == null || errors.Count == 0) {
                return true;
            }

            TaskDialog.Show("BIM", $"{string.Join($"{Environment.NewLine}", errors)}");
            return false;
        }

        private void InitializePlacing(RevitRepository revitRepository, IEnumerable<OpeningPlacer> placers) {
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
            using(var t = revitRepository.GetTransaction("Расстановка заданий")) {
                int count = 0;
                foreach(var p in placers) {
                    p.Place();
                    progress.Report(count++);
                    ct.ThrowIfCancellationRequested();
                }
                t.Commit();
            }
        }

        private void InitializeReport(RevitRepository revitRepository, IEnumerable<UnplacedClashModel> clashes) {
            if(!clashes.Any()) {
                return;
            }
            var viewModel = new ClashesViewModel(revitRepository, clashes);
            var window = new ReportView() { DataContext = viewModel };
            var helper = new WindowInteropHelper(window) { Owner = revitRepository.UIApplication.MainWindowHandle };

            window.Show();
        }
    }
}