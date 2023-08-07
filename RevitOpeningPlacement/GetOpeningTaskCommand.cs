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
using RevitOpeningPlacement.Models.OpeningUnion;
using RevitOpeningPlacement.OpeningModels;
using RevitOpeningPlacement.ViewModels.Navigator;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement {
    [Transaction(TransactionMode.Manual)]
    public class GetOpeningTaskCommand : BasePluginCommand {
        public GetOpeningTaskCommand() {
            PluginName = "Навигатор по заданиям";
        }

        protected override void Execute(UIApplication uiApplication) {
            ExecuteCommand(uiApplication);
        }

        public void ExecuteCommand(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
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
            var incomingTasks = revitRepository
                .GetOpeningMepTasksIncoming()
                .Select(famInst => new OpeningMepTaskIncomingViewModel(new OpeningMepTaskIncoming(famInst)))
                .ToList();
            var navigatorViewModel = new OpeningsMepTaskIncomingViewModel(revitRepository, incomingTasks);

            var window = new NavigatorMepIncomingView() { Title = PluginName, DataContext = navigatorViewModel };
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

            window.Show();
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
            var outcomingTasks = revitRepository
                .GetOpeningsMepTasksOutcoming()
                .Select(famInst => new OpeningMepTaskOutcoming(famInst));
            var openingTaskOutcomingViewModels = new List<OpeningMepTaskOutcomingViewModel>();
            foreach(var outcomingTask in outcomingTasks) {
                outcomingTask.UpdateStatus();
                openingTaskOutcomingViewModels.Add(new OpeningMepTaskOutcomingViewModel(outcomingTask));
            };
            var navigatorViewModel = new OpeningsMepTaskOutcomingViewModel(revitRepository, openingTaskOutcomingViewModels);

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
            var navigatorModeDialog = new TaskDialog("Навигатор по заданиям") {
                MainInstruction = "Выбор режима навигатора"
            };
            navigatorModeDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,
                "Посмотреть исходящие задания");
            navigatorModeDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2,
                "Посмотреть входящие задания");
            navigatorModeDialog.CommonButtons = TaskDialogCommonButtons.Close;
            navigatorModeDialog.DefaultButton = TaskDialogResult.Close;
            TaskDialogResult result = navigatorModeDialog.Show();
            switch(result) {
                case TaskDialogResult.CommandLink1:
                return NavigatorMode.Outgoing;
                case TaskDialogResult.CommandLink2:
                return NavigatorMode.Incoming;
                default:
                return NavigatorMode.NotDefined;
            }
        }

        private List<OpeningsGroup> GetOpeningsGroupsMepTasksOutcoming(RevitRepository revitRepository, UnionGroupsConfigurator configurator) {
            var groups = new List<OpeningsGroup>();
            using(var pb = GetPlatformService<IProgressDialogService>()) {
                IProgress<int> progress;
                CancellationToken ct;
                var maxValue = revitRepository.GetPlacedOutcomingTasks().Count;
                InitializeProgress(maxValue, pb, out progress, out ct);

                groups.AddRange(configurator.GetGroupsMepTasksOutcoming(progress, ct));
            }
            return groups;
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