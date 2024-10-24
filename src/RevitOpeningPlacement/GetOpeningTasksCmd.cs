using System;
using System.Windows;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Navigator.Checkers;
using RevitOpeningPlacement.OpeningModels;
using RevitOpeningPlacement.Services;
using RevitOpeningPlacement.ViewModels.Navigator;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement {
    /// <summary>
    /// Команда для просмотра размещенных в текущем файле исходящих заданий на отверстия 
    /// и полученных из связей входящих заданий
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class GetOpeningTasksCmd : BasePluginCommand {
        public GetOpeningTasksCmd() {
            PluginName = "Навигатор по заданиям";
        }


        public void ExecuteCommand(UIApplication uiApplication) {
            Execute(uiApplication);
        }


        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<RevitClashDetective.Models.RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<RevitEventHandler>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<ParameterFilterProvider>()
                    .ToSelf()
                    .InSingletonScope();

                var repo = kernel.Get<RevitRepository>();

                if(!ModelCorrect(repo)) {
                    return;
                }
                if(!repo.ContinueIfNotAllLinksLoaded()) {
                    throw new OperationCanceledException();
                }
                GetOpeningsTask(kernel);
            }
        }


        /// <summary>
        /// Логика вывода окна навигатора по заданиям на отверстия в зависимости от раздела проекта
        /// </summary>
        private void GetOpeningsTask(IKernel kernel) {
            var revitRepository = kernel.Get<RevitRepository>();
            var docType = revitRepository.GetDocumentType();
            switch(docType) {
                case DocTypeEnum.AR:
                    GetOpeningsTaskInDocumentAR(kernel);
                    break;
                case DocTypeEnum.KR:
                    GetOpeningsTaskInDocumentKR(kernel);
                    break;
                case DocTypeEnum.MEP:
                    GetOpeningsTaskInDocumentMEP(kernel);
                    break;
                case DocTypeEnum.KOORD:
                    GetOpeningsTaskInDocumentKoord(kernel);
                    break;
                default:
                    GetOpeningsTaskInDocumentNotDefined(kernel);
                    break;
            }
        }

        /// <summary>
        /// Логика вывода окна навигатора по заданиям на отверстия в файле архитектуры
        /// </summary>
        private void GetOpeningsTaskInDocumentAR(IKernel kernel) {
            kernel.Bind<IConstantsProvider>()
                .To<ConstantsProvider>()
                .InSingletonScope();
            kernel.Bind<ArchitectureNavigatorForIncomingTasksViewModel>()
                .ToSelf()
                .InSingletonScope();
            kernel.Bind<NavigatorMepIncomingView>()
                .ToSelf()
                .InSingletonScope()
                .WithPropertyValue(nameof(Window.DataContext),
                    c => c.Kernel.Get<ArchitectureNavigatorForIncomingTasksViewModel>())
                .WithPropertyValue(nameof(Window.Title), PluginName);

            var window = kernel.Get<NavigatorMepIncomingView>();
            var uiApplication = kernel.Get<UIApplication>();
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };
            window.Show();
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

        private KrNavigatorMode GetKrNavigatorMode() {
            var navigatorModeDialog = new TaskDialog("Навигатор по заданиям для КР") {
                MainInstruction = "Режим навигатора:"
            };
            navigatorModeDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Задания от АР");
            navigatorModeDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Задания от ВИС");
            navigatorModeDialog.CommonButtons = TaskDialogCommonButtons.Close;
            navigatorModeDialog.DefaultButton = TaskDialogResult.Close;
            TaskDialogResult result = navigatorModeDialog.Show();
            switch(result) {
                case TaskDialogResult.CommandLink1:
                    return KrNavigatorMode.IncomingAr;
                case TaskDialogResult.CommandLink2:
                    return KrNavigatorMode.IncomingMep;
                default:
                    throw new OperationCanceledException();
            }
        }

        /// <summary>
        /// Логика вывода окна навигатора по заданиям на отверстия в файле несущих конструкций
        /// </summary>
        private void GetOpeningsTaskInDocumentKR(IKernel kernel) {
            kernel.Bind<OpeningRealsKrConfig>()
                .ToMethod(c => {
                    var repo = c.Kernel.Get<RevitRepository>();
                    return OpeningRealsKrConfig.GetOpeningConfig(repo.Doc);
                });

            var navigatorMode = GetKrNavigatorMode();
            var config = kernel.Get<OpeningRealsKrConfig>();
            switch(navigatorMode) {
                case KrNavigatorMode.IncomingAr: {
                    config.PlacementType = OpeningRealKrPlacementType.PlaceByAr;
                    config.SaveProjectConfig();
                    break;
                }
                case KrNavigatorMode.IncomingMep: {
                    config.PlacementType = OpeningRealKrPlacementType.PlaceByMep;
                    config.SaveProjectConfig();
                    break;
                }
                default:
                    throw new OperationCanceledException();
            }

            kernel.Bind<IConstantsProvider>()
                .To<ConstantsProvider>()
                .InSingletonScope();
            kernel.Bind<ConstructureNavigatorForIncomingTasksViewModel>()
                .ToSelf()
                .InSingletonScope();
            kernel.Bind<NavigatorArIncomingView>()
                .ToSelf()
                .InSingletonScope()
                .WithPropertyValue(nameof(Window.DataContext),
                    c => c.Kernel.Get<ConstructureNavigatorForIncomingTasksViewModel>())
                .WithPropertyValue(nameof(Window.Title), PluginName);

            var window = kernel.Get<NavigatorArIncomingView>();
            var uiApplication = kernel.Get<UIApplication>();
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };
            window.Show();
        }

        private void GetOpeningsTaskInDocumentMEP(IKernel kernel) {
            kernel.Bind<OpeningConfig>()
                .ToMethod(c => {
                    var repo = c.Kernel.Get<RevitRepository>();
                    return OpeningConfig.GetOpeningConfig(repo.Doc);
                });
            kernel.Bind<IConstantsProvider>()
                .To<ConstantsProvider>()
                .InSingletonScope();
            kernel.Bind<ISolidProviderUtils>()
                .To<SolidProviderUtils>()
                .InSingletonScope();
            kernel.Bind<IOpeningInfoUpdater<OpeningMepTaskOutcoming>>()
                .To<MepTaskOutcomingInfoUpdater>()
                .InTransientScope();
            kernel.Bind<ILengthConverter>()
                .To<LengthConverterService>()
                .InSingletonScope();
            kernel.Bind<OutcomingTaskGeometryProvider>()
                .ToSelf()
                .InSingletonScope();
            kernel.Bind<GeometryUtils>()
                .ToSelf()
                .InSingletonScope();
            kernel.Bind<IOutcomingTaskOffsetFinder<Pipe>>()
                .To<PipeOffsetFinder>()
                .InTransientScope();
            kernel.Bind<IOutcomingTaskOffsetFinder<Duct>>()
                .To<DuctOffsetFinder>()
                .InTransientScope();
            kernel.Bind<IOutcomingTaskOffsetFinder<Conduit>>()
                .To<ConduitOffsetFinder>()
                .InTransientScope();
            kernel.Bind<IOutcomingTaskOffsetFinder<CableTray>>()
                .To<CableTrayOffsetFinder>()
                .InTransientScope();
            kernel.Bind<IOutcomingTaskOffsetFinder<FamilyInstance>>()
                .To<FamilyInstanceOffsetFinder>()
                .InTransientScope();
            kernel.Bind<IOutcomingTaskOffsetFinder<Element>>()
                .To<ElementOffsetFinder>()
                .InTransientScope();

            kernel.Bind<MepNavigatorForOutcomingTasksViewModel>()
                .ToSelf()
                .InSingletonScope();
            kernel.Bind<NavigatorMepOutcomingView>()
                .ToSelf()
                .InSingletonScope()
                .WithPropertyValue(nameof(Window.DataContext),
                    c => c.Kernel.Get<MepNavigatorForOutcomingTasksViewModel>())
                .WithPropertyValue(nameof(Window.Title), PluginName);

            var window = kernel.Get<NavigatorMepOutcomingView>();
            var uiApplication = kernel.Get<UIApplication>();
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };
            window.Show();
        }

        /// <summary>
        /// Логика вывода окна навигатора по заданиям на отверстия в файле с неопределенным разделом проектирования
        /// </summary>
        private void GetOpeningsTaskInDocumentNotDefined(IKernel kernel) {
            var revitRepository = kernel.Get<RevitRepository>();
            TaskDialog.Show("BIM",
                $"Название файла: \"{revitRepository.GetDocumentName()}\" не удовлетворяет BIM стандарту А101. " +
                $"Скорректируйте название и запустите команду снова.");
            throw new OperationCanceledException();
        }

        /// <summary>
        /// Логика вывода окна навигатора по заданиям на отверстия в координационном файле
        /// </summary>
        private void GetOpeningsTaskInDocumentKoord(IKernel kernel) {
            var revitRepository = kernel.Get<RevitRepository>();
            TaskDialog.Show(
                "BIM",
                $"Команда не может быть запущена в координационном файле \"{revitRepository.GetDocumentName()}\"");
            throw new OperationCanceledException();
        }
    }

    /// <summary>
    /// Перечисление режимов навигатора по заданиям на отверстия в файле КР
    /// </summary>
    internal enum KrNavigatorMode {
        /// <summary>
        /// Просмотр входящих заданий от АР
        /// </summary>
        IncomingAr,
        /// <summary>
        /// Просмотр входящих заданий от ВИС
        /// </summary>
        IncomingMep
    }
}
