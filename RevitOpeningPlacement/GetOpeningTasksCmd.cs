using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.Navigator.Checkers;
using RevitOpeningPlacement.OpeningModels;
using RevitOpeningPlacement.ViewModels.Navigator;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement {
    /// <summary>
    /// Команда для просмотра размещенных в текущем файле исходящих заданий на отверстия 
    /// и полученных из связей входящих заданий
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class GetOpeningTasksCmd : BasePluginCommand {
        private const int _progressBarStepLarge = 100;
        private const int _progressBarStepSmall = 25;


        public GetOpeningTasksCmd() {
            PluginName = "Навигатор по заданиям";
        }


        public void ExecuteCommand(UIApplication uiApplication) {
            Execute(uiApplication);
        }


        protected override void Execute(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(
                uiApplication.Application,
                uiApplication.ActiveUIDocument.Document);

            if(!ModelCorrect(revitRepository)) {
                return;
            }
            GetOpeningsTask(uiApplication, revitRepository);
        }


        /// <summary>
        /// Логика вывода окна навигатора по заданиям на отверстия в зависимости от раздела проекта
        /// </summary>
        /// <param name="uiApplication"></param>
        /// <param name="revitRepository"></param>
        private void GetOpeningsTask(UIApplication uiApplication, RevitRepository revitRepository) {
            var docType = revitRepository.GetDocumentType();
            switch(docType) {
                case DocTypeEnum.AR:
                GetOpeningsTaskInDocumentAR(uiApplication, revitRepository);
                break;
                case DocTypeEnum.KR:
                GetOpeningsTaskInDocumentKR(uiApplication, revitRepository);
                break;
                case DocTypeEnum.MEP:
                GetOpeningsTaskInDocumentMEP(uiApplication, revitRepository);
                break;
                case DocTypeEnum.KOORD:
                GetOpeningsTaskInDocumentKoord(revitRepository);
                break;
                default:
                GetOpeningsTaskInDocumentNotDefined(revitRepository);
                break;
            }
        }

        /// <summary>
        /// Логика вывода окна навигатора по заданиям на отверстия в файле архитектуры
        /// </summary>
        /// <param name="uiApplication"></param>
        /// <param name="revitRepository"></param>
        private void GetOpeningsTaskInDocumentAR(UIApplication uiApplication, RevitRepository revitRepository) {
            GetIncomingTaskInDocAR(uiApplication, revitRepository);
        }

        /// <summary>
        /// Запуск окна навигатора по входящим заданиям на отверстия в файле архитектуры
        /// </summary>
        /// <param name="uiApplication"></param>
        /// <param name="revitRepository"></param>
        private void GetIncomingTaskInDocAR(UIApplication uiApplication, RevitRepository revitRepository) {
            if(!revitRepository.ContinueIfNotAllLinksLoaded()) {
                throw new OperationCanceledException();
            }
            ICollection<OpeningMepTaskIncoming> incomingTasks = revitRepository.GetOpeningsMepTasksIncoming();
            ICollection<OpeningRealAr> realOpenings = revitRepository.GetRealOpeningsAr();
            ICollection<ElementId> constructureElementsIds = revitRepository.GetConstructureElementsIds();
            ICollection<IMepLinkElementsProvider> mepLinks = revitRepository
                .GetMepLinks()
                .Select(link => new MepLinkElementsProvider(link) as IMepLinkElementsProvider)
                .ToHashSet();

            var incomingTasksViewModels = GetOpeningsMepIncomingTasksViewModels(
                incomingTasks,
                realOpenings,
                constructureElementsIds);
            var openingsRealViewModels = GetOpeningsRealArViewModels(mepLinks, realOpenings);

            var navigatorViewModel = new ArchitectureNavigatorForIncomingTasksViewModel(
                revitRepository,
                incomingTasksViewModels,
                openingsRealViewModels);

            var window = new NavigatorMepIncomingView() { Title = PluginName, DataContext = navigatorViewModel };
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

            window.Show();
        }

        private void GetIncomingTaskInDocKR(UIApplication uiApplication, RevitRepository revitRepository) {
            if(!revitRepository.ContinueIfNotAllLinksLoaded()) {
                throw new OperationCanceledException();
            }
            ICollection<OpeningArTaskIncoming> incomingTasks = revitRepository.GetOpeningsArTasksIncoming();
            ICollection<OpeningRealKr> realOpenings = revitRepository.GetRealOpeningsKr();
            ICollection<ElementId> constructureElementsIds = revitRepository.GetConstructureElementsIds();
            ICollection<IConstructureLinkElementsProvider> arLinks = revitRepository
                .GetArLinks()
                .Select(link => new ConstructureLinkElementsProvider(link) as IConstructureLinkElementsProvider)
                .ToHashSet();

            var incomingTasksViewModels = GetOpeningsArIncomingTasksViewModels(
                incomingTasks,
                realOpenings,
                constructureElementsIds);
            var openingsRealViewModels = GetOpeningsRealKrViewModels(arLinks, realOpenings);

            var navigatorViewModel = new ConstructureNavigatorForIncomingTasksViewModel(
                revitRepository,
                incomingTasksViewModels,
                openingsRealViewModels);

            var window = new NavigatorArIncomingView() { Title = PluginName, DataContext = navigatorViewModel };
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

            window.Show();
        }

        /// <summary>
        /// Возвращает коллекцию моделей представления для входящих заданий на отверстия из АР
        /// </summary>
        /// <param name="incomingTasks">Входящие задания на отверстия из связей</param>
        /// <param name="realOpenings">Чистовые отверстия из текущего документа</param>
        /// <param name="constructureElementsIds">Элементы конструкций из текущего документа</param>
        /// <returns></returns>
        private ICollection<OpeningArTaskIncomingViewModel> GetOpeningsArIncomingTasksViewModels(
            ICollection<OpeningArTaskIncoming> incomingTasks,
            ICollection<OpeningRealKr> realOpenings,
            ICollection<ElementId> constructureElementsIds) {

            var incomintTasksViewModels = new HashSet<OpeningArTaskIncomingViewModel>();

            using(var pb = GetPlatformService<IProgressDialogService>()) {
                pb.StepValue = _progressBarStepSmall;
                pb.DisplayTitleFormat = "Анализ заданий... [{0}]\\[{1}]";
                var progress = pb.CreateProgress();
                pb.MaxValue = incomingTasks.Count;
                var ct = pb.CreateCancellationToken();
                pb.Show();

                int i = 0;
                foreach(var incomingTask in incomingTasks) {
                    ct.ThrowIfCancellationRequested();
                    progress.Report(i);
                    incomingTask.UpdateStatus(realOpenings, constructureElementsIds);
                    incomintTasksViewModels.Add(new OpeningArTaskIncomingViewModel(incomingTask));
                    i++;
                }
            }
            return incomintTasksViewModels;
        }

        /// <summary>
        /// Возвращает коллекцию моделей представления для входящих заданий на отверстия из ВИС
        /// </summary>
        /// <param name="incomingTasks">Входящие задания на отверстия из связей</param>
        /// <param name="realOpenings">Чистовые отверстия из текущего документа</param>
        /// <param name="constructureElementsIds">Элементы конструкций из текущего документа</param>
        /// <returns></returns>
        private ICollection<OpeningMepTaskIncomingViewModel> GetOpeningsMepIncomingTasksViewModels(
            ICollection<OpeningMepTaskIncoming> incomingTasks,
            ICollection<OpeningRealAr> realOpenings,
            ICollection<ElementId> constructureElementsIds) {

            var incomingTasksViewModels = new HashSet<OpeningMepTaskIncomingViewModel>();

            using(var pb = GetPlatformService<IProgressDialogService>()) {
                pb.StepValue = _progressBarStepLarge;
                pb.DisplayTitleFormat = "Анализ заданий... [{0}\\{1}]";
                var progress = pb.CreateProgress();
                pb.MaxValue = incomingTasks.Count;
                var ct = pb.CreateCancellationToken();
                pb.Show();

                int i = 0;
                foreach(var incomingTask in incomingTasks) {
                    ct.ThrowIfCancellationRequested();
                    progress.Report(i);
                    try {
                        incomingTask.UpdateStatusAndHostName(realOpenings, constructureElementsIds);
                    } catch(ArgumentException) {
                        //не удалось получить солид у задания на отверстие. Например, если его толщина равна 0
                        continue;
                    }
                    incomingTasksViewModels.Add(new OpeningMepTaskIncomingViewModel(incomingTask));
                    i++;
                }
            }
            return incomingTasksViewModels;
        }

        /// <summary>
        /// Возвращает коллекцию моделей представления чистовых отверстий, размещенных в активном документа КР
        /// </summary>
        /// <param name="arLinks">Связи АР</param>
        /// <param name="openingsReal">Чистовые отверстия, размещенные в активном документа КР</param>
        /// <returns></returns>
        private ICollection<OpeningRealKrViewModel> GetOpeningsRealKrViewModels(
            ICollection<IConstructureLinkElementsProvider> arLinks,
            ICollection<OpeningRealKr> openingsReal) {

            var openingsRealViewModels = new HashSet<OpeningRealKrViewModel>();

            using(var pb = GetPlatformService<IProgressDialogService>()) {
                pb.StepValue = _progressBarStepSmall;
                pb.DisplayTitleFormat = "Анализ отверстий... [{0}]\\[{1}]";
                var progress = pb.CreateProgress();
                pb.MaxValue = openingsReal.Count;
                var ct = pb.CreateCancellationToken();
                pb.Show();

                int i = 0;
                foreach(var openingReal in openingsReal) {
                    ct.ThrowIfCancellationRequested();
                    progress.Report(i);
                    openingReal.UpdateStatus(arLinks);
                    if(openingReal.Status != OpeningModels.Enums.OpeningRealStatus.Correct) {
                        openingsRealViewModels.Add(new OpeningRealKrViewModel(openingReal));
                    }
                    i++;
                }
            }
            return openingsRealViewModels;
        }

        /// <summary>
        /// Возвращает коллекцию моделей представления чистовых отверстий, размещенных в активном документа АР
        /// </summary>
        /// <param name="mepLinks">Связи ВИС</param>
        /// <param name="openingsReal">Чистовые отверстия, размещенные в активном документе АР</param>
        /// <returns></returns>
        private ICollection<OpeningRealArViewModel> GetOpeningsRealArViewModels(
            ICollection<IMepLinkElementsProvider> mepLinks,
            ICollection<OpeningRealAr> openingsReal) {

            var openingsRealViewModels = new HashSet<OpeningRealArViewModel>();

            using(var pb = GetPlatformService<IProgressDialogService>()) {
                pb.StepValue = _progressBarStepSmall;
                pb.DisplayTitleFormat = "Анализ отверстий... [{0}\\{1}]";
                var progress = pb.CreateProgress();
                pb.MaxValue = openingsReal.Count;
                var ct = pb.CreateCancellationToken();
                pb.Show();

                var i = 0;
                foreach(var openingReal in openingsReal) {
                    ct.ThrowIfCancellationRequested();
                    progress.Report(i);
                    openingReal.UpdateStatus(mepLinks);
                    if(openingReal.Status != OpeningModels.Enums.OpeningRealStatus.Correct) {
                        openingsRealViewModels.Add(new OpeningRealArViewModel(openingReal));
                    }
                    i++;
                }
            }
            return openingsRealViewModels;
        }

        private bool ModelCorrect(RevitRepository revitRepository) {
            var checker = new NavigatorCheckers(revitRepository);
            var errors = checker.GetErrorTexts();
            if(errors == null || errors.Count == 0) {
                return true;
            }

            TaskDialog.Show("BIM", $"{string.Join($"{Environment.NewLine}", errors)}");
            return false;
        }

        /// <summary>
        /// Логика вывода окна навигатора по заданиям на отверстия в файле несущих конструкций
        /// </summary>
        /// <param name="uiApplication"></param>
        /// <param name="revitRepository"></param>
        private void GetOpeningsTaskInDocumentKR(UIApplication uiApplication, RevitRepository revitRepository) {
            GetIncomingTaskInDocKR(uiApplication, revitRepository);
        }

        /// <summary>
        /// Логика вывода окна навигатора по заданиям на отверстия в файле инженерных систем
        /// </summary>
        /// <param name="uiApplication"></param>
        /// <param name="revitRepository"></param>
        private void GetOpeningsTaskInDocumentMEP(UIApplication uiApplication, RevitRepository revitRepository) {
            if(!revitRepository.ContinueIfNotAllLinksLoaded()) {
                throw new OperationCanceledException();
            }
            var outcomingTasks = revitRepository.GetOpeningsMepTasksOutcoming();
            IList<ElementId> outcomingTasksIds = outcomingTasks.Select(task => task.Id).ToList();
            var mepElementsIds = revitRepository.GetMepElementsIds();
            var constructureLinks = GetLinkProviders(revitRepository);
            var openingTaskOutcomingViewModels = GetMepTaskOutcomingViewModels(
                outcomingTasks,
                ref outcomingTasksIds,
                mepElementsIds,
                constructureLinks);

            var navigatorViewModel = new MepNavigatorForOutcomingTasksViewModel(
                revitRepository,
                openingTaskOutcomingViewModels);

            var window = new NavigatorMepOutcomingView() { Title = PluginName, DataContext = navigatorViewModel };
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

            window.Show();
        }

        private ICollection<OpeningMepTaskOutcomingViewModel> GetMepTaskOutcomingViewModels(
            ICollection<OpeningMepTaskOutcoming> outcomingTasks,
            ref IList<ElementId> outcomingTasksIds,
            ICollection<ElementId> mepElementsIds,
            ICollection<IConstructureLinkElementsProvider> constructureLinks
            ) {
            var openingTaskOutcomingViewModels = new List<OpeningMepTaskOutcomingViewModel>();

            using(var pb = GetPlatformService<IProgressDialogService>()) {
                pb.StepValue = _progressBarStepLarge;
                pb.DisplayTitleFormat = "Анализ заданий... [{0}\\{1}]";
                var progress = pb.CreateProgress();
                pb.MaxValue = outcomingTasks.Count;
                var ct = pb.CreateCancellationToken();
                pb.Show();

                int i = 0;
                foreach(var outcomingTask in outcomingTasks) {
                    ct.ThrowIfCancellationRequested();
                    progress.Report(i);
                    outcomingTask.UpdateStatus(ref outcomingTasksIds, mepElementsIds, constructureLinks);
                    openingTaskOutcomingViewModels.Add(new OpeningMepTaskOutcomingViewModel(outcomingTask));
                    i++;
                }
            }
            return openingTaskOutcomingViewModels;
        }

        private ICollection<IConstructureLinkElementsProvider> GetLinkProviders(RevitRepository revitRepository) {
            var constructureLinks = revitRepository.GetConstructureLinks();
            List<IConstructureLinkElementsProvider> providers = new List<IConstructureLinkElementsProvider>();

            using(var pb = GetPlatformService<IProgressDialogService>()) {
                pb.StepValue = 1;
                pb.DisplayTitleFormat = "Анализ связей... [{0}\\{1}]";
                var progress = pb.CreateProgress();
                pb.MaxValue = constructureLinks.Count;
                var ct = pb.CreateCancellationToken();
                pb.Show();
                int i = 0;
                foreach(var constructureLink in constructureLinks) {
                    ct.ThrowIfCancellationRequested();
                    providers.Add(new ConstructureLinkElementsProvider(constructureLink));
                    progress.Report(i++);
                }
            }
            return providers;
        }

        /// <summary>
        /// Логика вывода окна навигатора по заданиям на отверстия в файле с неопределенным разделом проектирования
        /// </summary>
        /// <param name="revitRepository"></param>
        private void GetOpeningsTaskInDocumentNotDefined(RevitRepository revitRepository) {
            TaskDialog.Show("BIM",
                $"Название файла: \"{revitRepository.GetDocumentName()}\" не удовлетворяет BIM стандарту А101. " +
                $"Скорректируйте название и запустите команду снова.");
            throw new OperationCanceledException();
        }

        /// <summary>
        /// Логика вывода окна навигатора по заданиям на отверстия в координационном файле
        /// </summary>
        /// <param name="revitRepository"></param>
        private void GetOpeningsTaskInDocumentKoord(RevitRepository revitRepository) {
            TaskDialog.Show(
                "BIM",
                $"Команда не может быть запущена в координационном файле \"{revitRepository.GetDocumentName()}\"");
            throw new OperationCanceledException();
        }
    }

    /// <summary>
    /// Перечисление режимов навигатора по заданиям на отверстиям
    /// </summary>
    internal enum NavigatorMode {
        /// <summary>
        /// Просмотр исходящих заданий
        /// </summary>
        Outgoing,
        /// <summary>
        /// Просмотр входящих заданий
        /// </summary>
        Incoming,
        /// <summary>
        /// Режим просмотра не задан
        /// </summary>
        NotDefined
    }
}
