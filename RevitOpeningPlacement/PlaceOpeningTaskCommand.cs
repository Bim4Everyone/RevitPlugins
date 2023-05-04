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
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.Checkers;
using RevitOpeningPlacement.OpeningModels;
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
            var placedOpeningTasks = revitRepository.GetPlacedOutcomingTasks();
            var newOpenings = new List<FamilyInstance>();

            using(var t = revitRepository.GetTransaction("Расстановка заданий")) {
                int count = 0;
                foreach(var p in placers) {
                    var newOpening = p.Place();
                    newOpenings.Add(newOpening);

                    progress.Report(count++);
                    ct.ThrowIfCancellationRequested();
                }
                //Отсрочить показ предупреждений до завершения следующей транзакции - удаления дублирующих отверстий в RemoveAlreadyPlacedOpenings
                FailureHandlingOptions options = t.GetFailureHandlingOptions()
                    .SetForcedModalHandling(false)
                    .SetDelayedMiniWarnings(true);
                t.Commit(options);
            }

            //Удаление дублирующих заданий на отверстия нужно начинать в отдельной транзакции после завершения транзакции создания заданий на отверстия,
            //т.к. свойство Location у элемента FamilyInstance, созданного внутри транзакции, может быть не актуально (установлено в (0,0,0) несмотря на реальное расположение),
            //а после завершения транзакции актуализируется
            RemoveAlreadyPlacedOpenings(revitRepository, newOpenings.Select(o => new OpeningTaskOutcoming(o)).ToList(), placedOpeningTasks);
        }

        private void RemoveAlreadyPlacedOpenings(RevitRepository revitRepository, ICollection<OpeningTaskOutcoming> newOpenings, ICollection<OpeningTaskOutcoming> alreadyPlacedOpenings) {
            using(var t = revitRepository.GetTransaction("Удаление дублирующих заданий")) {
                foreach(var newOpening in newOpenings) {
                    if(IsAlreadyPlaced(newOpening, alreadyPlacedOpenings)) {
                        revitRepository.DeleteElement(newOpening.Id);
                    }
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

        /// <summary>
        /// Проверяет, размещено ли уже такое же задание на отверстие в проекте. Под "таким" же понимается семейство задания на отверстие с координатами
        /// </summary>
        /// <param name="newOpening">Новое задание на отверстие</param>
        /// <param name="placedOpenings">Существующие задания на отверстия в проекте</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private bool IsAlreadyPlaced(OpeningTaskOutcoming newOpening, ICollection<OpeningTaskOutcoming> placedOpenings) {
            if(newOpening == null) {
                throw new ArgumentNullException(nameof(newOpening));
            }
            var toleranceCubeDiagonal = Math.Sqrt(XYZExtension.FeetRound * XYZExtension.FeetRound * XYZExtension.FeetRound);

            var closestPlacedOpenings = placedOpenings.Where(placedOpening =>
                placedOpening.Location
                .DistanceTo(placedOpening.Location) <= toleranceCubeDiagonal);

            foreach(OpeningTaskOutcoming placedOpening in closestPlacedOpenings) {
                if(placedOpening.EqualsSolid(newOpening.GetSolid(), XYZExtension.FeetRound)) {
                    return true;
                }
            }
            return false;
        }
    }
}