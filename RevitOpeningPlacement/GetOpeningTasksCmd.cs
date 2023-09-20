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
    /// Команда для просмотра размещенных в текущем файле исходящих заданий на отверстия и полученных из связей входящих заданий
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class GetOpeningTasksCmd : BasePluginCommand {
        private const int _progressBarStepLarge = 100;
        private const int _progressBarStepSmall = 25;


        public GetOpeningTasksCmd() {
            PluginName = "Навигатор по заданиям";
        }

        protected override void Execute(UIApplication uiApplication) {
            ExecuteCommand(uiApplication);
        }

        public void ExecuteCommand(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);

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
                case DocTypeEnum.NotDefined:
                GetOpeningsTaskInDocumentNotDefined(revitRepository);
                break;
                default:
                throw new ArgumentException(nameof(docType));
            }
        }

        /// <summary>
        /// Логика вывода окна навигатора по заданиям на отверстия в файле архитектуры
        /// </summary>
        /// <param name="uiApplication"></param>
        /// <param name="revitRepository"></param>
        private void GetOpeningsTaskInDocumentAR(UIApplication uiApplication, RevitRepository revitRepository) {
            var navigatorMode = GetNavigatorModeFromUser();
            switch(navigatorMode) {
                case NavigatorMode.Outgoing:
                GetOutgoingTaskInDocAR(uiApplication, revitRepository);
                break;
                case NavigatorMode.Incoming:
                GetIncomingTaskInDocAR(uiApplication, revitRepository);
                break;
                case NavigatorMode.NotDefined:
                throw new OperationCanceledException();
                default:
                throw new ArgumentException(nameof(navigatorMode));
            }
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
            ICollection<OpeningReal> realOpenings = revitRepository.GetRealOpenings();
            ICollection<ElementId> constructureElementsIds = revitRepository.GetConstructureElementsIds();
            ICollection<IMepLinkElementsProvider> mepLinks = revitRepository
                .GetMepLinks()
                .Select(link => new MepLinkElementsProvider(link) as IMepLinkElementsProvider)
                .ToHashSet();

            var incomingTasksViewModels = GetOpeningsMepIncomingTasksVM(incomingTasks, realOpenings, constructureElementsIds);
            var openingsRealViewModels = GetOpeningsRealVM(mepLinks, realOpenings);

            var navigatorViewModel = new ArchitectureNavigatorForIncomingTasksViewModel(
                revitRepository,
                incomingTasksViewModels,
                openingsRealViewModels);

            var window = new NavigatorMepIncomingView() { Title = PluginName, DataContext = navigatorViewModel };
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

            window.Show();
        }

        /// <summary>
        /// Получает коллекцию моделей представления для входящих заданий на отверстия
        /// </summary>
        /// <param name="incomingTasks">Входящие задания на отверстия из связей</param>
        /// <param name="realOpenings">Чистовые отверстия из текущего документа</param>
        /// <param name="constructureElementsIds">Элементы конструкций из текущего документа</param>
        /// <returns></returns>
        private ICollection<OpeningMepTaskIncomingViewModel> GetOpeningsMepIncomingTasksVM(
            ICollection<OpeningMepTaskIncoming> incomingTasks,
            ICollection<OpeningReal> realOpenings,
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

        private ICollection<OpeningRealViewModel> GetOpeningsRealVM(
            ICollection<IMepLinkElementsProvider> mepLinks,
            ICollection<OpeningReal> openingsReal) {

            var openingsRealViewModels = new HashSet<OpeningRealViewModel>();

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
                        openingsRealViewModels.Add(new OpeningRealViewModel(openingReal));
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
        /// Запуск окна навигатора по исходящим заданиям на отверстия в файле архитектуры
        /// </summary>
        /// <param name="uiApplication"></param>
        /// <param name="revitRepository"></param>
        private void GetOutgoingTaskInDocAR(UIApplication uiApplication, RevitRepository revitRepository) {
            TaskDialog.Show("Навигатор по заданиям на отверстия", "Навигатор по исходящим заданиям на отверстия в файле АР находится в разработке");
        }

        /// <summary>
        /// Логика вывода окна навигатора по заданиям на отверстия в файле несущих конструкций
        /// </summary>
        /// <param name="uiApplication"></param>
        /// <param name="revitRepository"></param>
        private void GetOpeningsTaskInDocumentKR(UIApplication uiApplication, RevitRepository revitRepository) {
            GetIncomingTaskInDocAR(uiApplication, revitRepository);
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
            IList<ElementId> outcomingTasksIds = outcomingTasks.Select(task => new ElementId(task.Id)).ToList();
            var mepElementsIds = revitRepository.GetMepElementsIds();
            var openingTaskOutcomingViewModels = new List<OpeningMepTaskOutcomingViewModel>();
            var constructureLinks = revitRepository.GetConstructureLinks().Select(link => new ConstructureLinkElementsProvider(link) as IConstructureLinkElementsProvider).ToHashSet();

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
            var navigatorViewModel = new MepNavigatorForOutcomingTasksViewModel(revitRepository, openingTaskOutcomingViewModels);

            var window = new NavigatorMepOutcomingView() { Title = PluginName, DataContext = navigatorViewModel };
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

            window.Show();
        }

        /// <summary>
        /// Логика вывода окна навигатора по заданиям на отверстия в файле с неопределенным разделом проектирования
        /// </summary>
        /// <param name="revitRepository"></param>
        private void GetOpeningsTaskInDocumentNotDefined(RevitRepository revitRepository) {
            TaskDialog.Show("BIM", $"Название файла: \"{revitRepository.GetDocumentName()}\" не удовлетворяет BIM стандарту А101. " +
                $"Скорректируйте название и запустите команду снова.");
            throw new OperationCanceledException();
        }

        /// <summary>
        /// Логика вывода окна навигатора по заданиям на отверстия в координационном файле
        /// </summary>
        /// <param name="revitRepository"></param>
        private void GetOpeningsTaskInDocumentKoord(RevitRepository revitRepository) {
            TaskDialog.Show("BIM", $"Команда не может быть запущена в координационном файле \"{revitRepository.GetDocumentName()}\"");
            throw new OperationCanceledException();
        }

        /// <summary>
        /// Выбор пользователем режима навигатора по заданиям на отверстия
        /// </summary>
        /// <returns></returns>
        private NavigatorMode GetNavigatorModeFromUser() {
            return NavigatorMode.Incoming; //TODO убрать после реализации создания заданий на отверстия в файле АР

            //var navigatorModeDialog = new TaskDialog("Навигатор по заданиям") {
            //    MainInstruction = "Выбор режима навигатора"
            //};
            //navigatorModeDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,
            //    "Посмотреть исходящие задания");
            //navigatorModeDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2,
            //    "Посмотреть входящие задания");
            //navigatorModeDialog.CommonButtons = TaskDialogCommonButtons.Close;
            //navigatorModeDialog.DefaultButton = TaskDialogResult.Close;
            //TaskDialogResult result = navigatorModeDialog.Show();
            //switch(result) {
            //    case TaskDialogResult.CommandLink1:
            //    return NavigatorMode.Outgoing;
            //    case TaskDialogResult.CommandLink2:
            //    return NavigatorMode.Incoming;
            //    default:
            //    return NavigatorMode.NotDefined;
            //}
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