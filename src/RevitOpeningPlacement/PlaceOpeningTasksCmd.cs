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
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.Checkers;
using RevitOpeningPlacement.Models.OpeningUnion;
using RevitOpeningPlacement.OpeningModels;
using RevitOpeningPlacement.ViewModels.ReportViewModel;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement {
    /// <summary>
    /// Команда для размещения заданий на отверстия в файле ВИС.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class PlaceOpeningTasksCmd : BasePluginCommand {
        private readonly int _progressBarStepValue = 25;

        /// <summary>
        /// Id элементов которые выдают предупреждение дублирования в Revit и которые нужно удалить
        /// </summary>
        private readonly HashSet<ElementId> _duplicatedInstancesToRemoveIds = new HashSet<ElementId>();


        public PlaceOpeningTasksCmd() {
            PluginName = "Расстановка заданий на отверстия";
        }


        protected override void Execute(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            PlaceOpeningTasks(uiApplication, revitRepository, Array.Empty<ElementId>());
        }

        private protected void PlaceOpeningTasks(UIApplication uiApplication, RevitRepository revitRepository, ElementId[] mepElements) {
            _duplicatedInstancesToRemoveIds.Clear();
            if(!revitRepository.ContinueIfNotAllLinksLoaded()) {
                throw new OperationCanceledException();
            }
            if(!ModelCorrect(revitRepository)) {
                throw new OperationCanceledException();
            }
            if(!revitRepository.ContinueIfTaskFamiliesNotLatest()) {
                throw new OperationCanceledException();
            }
            var openingConfig = OpeningConfig.GetOpeningConfig(revitRepository.Doc);
            if(openingConfig.Categories.Count > 0) {
                var placementConfigurator = new PlacementConfigurator(revitRepository, openingConfig.Categories);
                var placers = placementConfigurator.GetPlacersMepOutcomingTasks(mepElements)
                                                   .ToList();
                uiApplication.Application.FailuresProcessing += FailureProcessor;
                try {
                    var unplacedClashes = InitializePlacing(revitRepository, placers)
                        .Concat(placementConfigurator.GetUnplacedClashes());
                    InitializeReport(revitRepository, unplacedClashes);
                } finally {
                    uiApplication.Application.FailuresProcessing -= FailureProcessor;
                }
            }
            _duplicatedInstancesToRemoveIds.Clear();
        }

        private bool ModelCorrect(RevitRepository revitRepository) {
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
            HashSet<FamilyInstance> placedFamInstances = new HashSet<FamilyInstance>();
            List<UnplacedClashModel> unplacedClashes = new List<UnplacedClashModel>();

            using(var t = revitRepository.GetTransaction("Расстановка заданий")) {
                int count = 0;
                foreach(var p in placers) {
                    ct.ThrowIfCancellationRequested();
                    progress.Report(count);
                    try {
                        var newOpening = p.Place();
                        placedFamInstances.Add(newOpening);
                    } catch(OpeningNotPlacedException e) {
                        var clashModel = p.ClashModel;
                        if(!(clashModel is null)) {
                            unplacedClashes.Add(new UnplacedClashModel() { Message = e.Message, Clash = clashModel });
                        }
                    }
                    count++;
                }
                var options = t.GetFailureHandlingOptions()
                    .SetForcedModalHandling(false)
                    .SetDelayedMiniWarnings(true);
                t.Commit(options);
            }
            // создание экземпляров классов OpeningMepTaskOutcoming после коммита транзакции,
            // потому что параметры экземпляров семейств до коммита еще не сохранены в модели и не могут быть получены конструктором OpeningMepTaskOutcoming
            HashSet<OpeningMepTaskOutcoming> newOpenings = placedFamInstances.Select(famInst => new OpeningMepTaskOutcoming(famInst)).ToHashSet();

            //Удаление дублирующих заданий на отверстия нужно начинать в отдельной транзакции после завершения транзакции создания заданий на отверстия,
            //т.к. свойство Location у элемента FamilyInstance, созданного внутри транзакции, может быть не актуально (установлено в (0,0,0) несмотря на реальное расположение),
            //а после завершения транзакции актуализируется
            if(placedOpeningTasks.Count > 0) {
                var newOpeningsNotDeleted = InitializeRemoving(revitRepository, newOpenings, placedOpeningTasks);
                if(newOpeningsNotDeleted.Count > 1) {
                    InitializeUnion(revitRepository, newOpeningsNotDeleted);
                }
            } else {
                // инициализация удаления дубликатов только что созданных заданий на отверстия здесь не запускается,
                // потому что дубликаты будут объединены в одно задание
                InitializeUnion(revitRepository, newOpenings);
            }
            return unplacedClashes;
        }

        private ICollection<OpeningMepTaskOutcoming> InitializeRemoving(
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

                return RemoveAlreadyPlacedOpenings(
                    revitRepository,
                    newOpenings,
                    alreadyPlacedOpenings,
                    progressRemove,
                    ctRemove);
            }
        }

        private ICollection<OpeningMepTaskOutcoming> RemoveAlreadyPlacedOpenings(
            RevitRepository revitRepository,
            ICollection<OpeningMepTaskOutcoming> newOpenings,
            ICollection<OpeningMepTaskOutcoming> alreadyPlacedOpenings,
            IProgress<int> progress,
            CancellationToken ct) {

            HashSet<OpeningMepTaskOutcoming> newOpeningsNotDeleted = new HashSet<OpeningMepTaskOutcoming>();
            if(alreadyPlacedOpenings.Count > 0) {
                using(var t = revitRepository.GetTransaction("Удаление дублирующих заданий")) {
                    int count = 0;
                    foreach(var newOpening in newOpenings) {
                        ct.ThrowIfCancellationRequested();
                        progress.Report(count);
                        bool deleteNewOpening = _duplicatedInstancesToRemoveIds.Contains(newOpening.Id) || newOpening.IsAlreadyPlaced(alreadyPlacedOpenings);
                        if(deleteNewOpening) {
                            revitRepository.DeleteElement(newOpening.Id);
                        } else {
                            newOpeningsNotDeleted.Add(newOpening);
                        }
                        count++;
                    }
                    t.Commit();
                }
                return newOpeningsNotDeleted;
            } else {
                newOpeningsNotDeleted.UnionWith(newOpenings);
                return newOpeningsNotDeleted;
            }
        }

        private void InitializeUnion(
            RevitRepository revitRepository,
            ICollection<OpeningMepTaskOutcoming> newOpeningsForUnion
            ) {
            using(var pb = GetPlatformService<IProgressDialogService>()) {
                pb.StepValue = _progressBarStepValue;
                pb.DisplayTitleFormat = "Объединение касающихся... [{0}\\{1}]";
                var progressUnite = pb.CreateProgress();
                pb.MaxValue = newOpeningsForUnion.Count;
                var ctUnite = pb.CreateCancellationToken();
                pb.Show();

                UniteTouchingOpenings(progressUnite, ctUnite, revitRepository, newOpeningsForUnion);
            }
        }

        private void UniteTouchingOpenings(IProgress<int> progress, CancellationToken ct, RevitRepository revitRepository, ICollection<OpeningMepTaskOutcoming> newOpeningsForUnion) {
            ICollection<OpeningsGroup> groups = new MultilayerOpeningsGroupsProvider(revitRepository).GetOpeningsGroups(newOpeningsForUnion);
            using(var t = revitRepository.GetTransaction("Объединение многослойных заданий")) {

                int count = 0;
                foreach(OpeningsGroup group in groups) {
                    ct.ThrowIfCancellationRequested();
                    progress.Report(count);
                    try {
                        OpeningPlacer placer = group.GetOpeningPlacer(revitRepository);
                        FamilyInstance unitedOpening = placer.Place();

                        if(unitedOpening != null) {
                            foreach(OpeningMepTaskOutcoming openingTask in group.Elements) {
                                revitRepository.DeleteElement(openingTask.Id);
                            }
                        }

                    } catch(ArgumentOutOfRangeException) {
                        continue;
                    } catch(InvalidOperationException) {
                        continue;
                    } catch(OpeningNotPlacedException) {
                        continue;
                    }
                    count++;
                }

                t.Commit();
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
                    // в дубликаты заносить все элементы, кроме самого старого, у которого значение Id наименьшее
                    var ids = fma.GetFailingElementIds().OrderBy(elId => elId.GetIdValue()).Skip(1);
                    foreach(var id in ids) {
                        _duplicatedInstancesToRemoveIds.Add(id);
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
