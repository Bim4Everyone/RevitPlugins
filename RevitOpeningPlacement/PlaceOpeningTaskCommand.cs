using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.Checkers;
using RevitOpeningPlacement.OpeningModels;
using RevitOpeningPlacement.ViewModels.ReportViewModel;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement {

    [Transaction(TransactionMode.Manual)]
    public class PlaceOpeningTaskCommand : BasePluginCommand {
        private readonly int _progressBarStepValue = 10;

        /// <summary>
        /// Список Id элементов которые выдают предупреждение дублирования в Revit
        /// </summary>
        private readonly List<int> _duplicatedInstancesIds = new List<int>();


        public PlaceOpeningTaskCommand() {
            PluginName = "Расстановка заданий на отверстия";
        }

        protected override void Execute(UIApplication uiApplication) {
            _duplicatedInstancesIds.Clear();
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            if(!revitRepository.ContinueIfNotAllLinksLoaded()) {
                throw new OperationCanceledException();
            }
            if(!CheckModel(revitRepository)) {
                return;
            }
            var openingConfig = OpeningConfig.GetOpeningConfig(revitRepository.Doc);
            if(openingConfig.Categories.Count > 0) {
                var placementConfigurator = new PlacementConfigurator(revitRepository, openingConfig.Categories);
                var placers = placementConfigurator.GetPlacersMepOutcomingTasks()
                                                   .ToList();
                uiApplication.Application.FailuresProcessing += FailureProcessor;
                var unplacedClashes = InitializePlacing(revitRepository, placers)
                    .Concat(placementConfigurator.GetUnplacedClashes());
                uiApplication.Application.FailuresProcessing -= FailureProcessor;
                InitializeReport(revitRepository, unplacedClashes);
            }
            _duplicatedInstancesIds.Clear();
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

        private IList<UnplacedClashModel> InitializePlacing(RevitRepository revitRepository, IEnumerable<OpeningPlacer> placers) {
            if(placers.Count() == 0) {
                return new List<UnplacedClashModel>();
            }
            using(var pb = GetPlatformService<IProgressDialogService>()) {
                pb.StepValue = _progressBarStepValue;
                pb.DisplayTitleFormat = "Расстановка отверстий... [{0}\\{1}]";
                var progress = pb.CreateProgress();
                pb.MaxValue = placers.Count();
                var ct = pb.CreateCancellationToken();
                pb.Show();

                return PlaceOpenings(progress, ct, revitRepository, placers);
            }
        }

        private IList<UnplacedClashModel> PlaceOpenings(IProgress<int> progress, CancellationToken ct, RevitRepository revitRepository, IEnumerable<OpeningPlacer> placers) {
            var placedOpeningTasks = revitRepository.GetPlacedOutcomingTasks();
            var newOpenings = new List<OpeningMepTaskOutcoming>();
            List<UnplacedClashModel> unplacedClashes = new List<UnplacedClashModel>();

            using(var t = revitRepository.GetTransaction("Расстановка заданий")) {
                int count = 0;
                int lastCount = 0;
                foreach(var p in placers) {
                    count++;
                    try {
                        var newOpening = p.Place();
                        newOpenings.Add(new OpeningMepTaskOutcoming(newOpening));
                    } catch(OpeningNotPlacedException e) {
                        var clashModel = p.ClashModel;
                        if(!(clashModel is null)) {
                            unplacedClashes.Add(new UnplacedClashModel() { Message = e.Message, Clash = clashModel });
                        }
                    }
                    if((count == 0) || ((count - lastCount) >= _progressBarStepValue)) {
                        progress.Report(count);
                        lastCount = count;
                    }
                    ct.ThrowIfCancellationRequested();
                }
                var options = t.GetFailureHandlingOptions()
                    .SetForcedModalHandling(false)
                    .SetDelayedMiniWarnings(true);
                t.Commit(options);
            }

            //Удаление дублирующих заданий на отверстия нужно начинать в отдельной транзакции после завершения транзакции создания заданий на отверстия,
            //т.к. свойство Location у элемента FamilyInstance, созданного внутри транзакции, может быть не актуально (установлено в (0,0,0) несмотря на реальное расположение),
            //а после завершения транзакции актуализируется
            if(placedOpeningTasks.Count > 0) {
                InitializeRemoving(revitRepository, newOpenings, placedOpeningTasks);
            }
            return unplacedClashes;
        }

        private void InitializeRemoving(
            RevitRepository revitRepository,
            ICollection<OpeningMepTaskOutcoming> newOpenings,
            ICollection<OpeningMepTaskOutcoming> alreadyPlacedOpenings) {
            using(var pb = GetPlatformService<IProgressDialogService>()) {
                pb.StepValue = _progressBarStepValue;
                pb.DisplayTitleFormat = "Проверка дублей... [{0}\\{1}]";
                var progressRemove = pb.CreateProgress();
                pb.MaxValue = newOpenings.Count;
                var ctRemove = pb.CreateCancellationToken();
                pb.Show();

                RemoveAlreadyPlacedOpenings(
                    revitRepository,
                    newOpenings,
                    alreadyPlacedOpenings,
                    progressRemove,
                    ctRemove);
            }
        }

        private void RemoveAlreadyPlacedOpenings(
            RevitRepository revitRepository,
            ICollection<OpeningMepTaskOutcoming> newOpenings,
            ICollection<OpeningMepTaskOutcoming> alreadyPlacedOpenings,
            IProgress<int> progress,
            CancellationToken ct) {
            if(alreadyPlacedOpenings.Count > 0) {
                using(var t = revitRepository.GetTransaction("Удаление дублирующих заданий")) {
                    int count = 0;
                    int lastCount = 0;
                    foreach(var newOpening in newOpenings) {
                        bool deleteNewOpening = _duplicatedInstancesIds.Contains(newOpening.Id) || newOpening.IsAlreadyPlaced(alreadyPlacedOpenings);
                        if(deleteNewOpening) {
                            revitRepository.DeleteElement(newOpening.Id);
                        }
                        count++;
                        if((count == 0) || ((count - lastCount) >= _progressBarStepValue)) {
                            progress.Report(count);
                            lastCount = count;
                        }
                        ct.ThrowIfCancellationRequested();
                    }
                    t.Commit();
                }
            }
        }

        /// <summary>
        /// Метод для подписки на событие <see cref="UIApplication.Application.FailuresProcessing"/>
        /// Используется для запоминания расставленных экземпляров семейств заданий на отверстия, которые дают дублирование с уже размещенными
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FailureProcessor(object sender, FailuresProcessingEventArgs e) {
            FailuresAccessor fas = e.GetFailuresAccessor();

            List<FailureMessageAccessor> fmas = fas.GetFailureMessages().ToList();

            foreach(FailureMessageAccessor fma in fmas) {
                var definition = fma.GetFailureDefinitionId();
                if(definition == BuiltInFailures.OverlapFailures.DuplicateInstances) {
                    var ids = fma.GetFailingElementIds().Select(id => id.IntegerValue);
                    foreach(var id in ids) {
                        if(!_duplicatedInstancesIds.Contains(id)) {
                            _duplicatedInstancesIds.Add(id);
                        }
                    }
                };
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