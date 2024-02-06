using System;
using System.Windows;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Bim4Everyone.Templates;
using dosymep.SimpleServices;

using Ninject;

using RevitSetLevelSection.Factories;
using RevitSetLevelSection.Factories.ElementPositions;
using RevitSetLevelSection.Factories.LevelProviders;
using RevitSetLevelSection.Models;
using RevitSetLevelSection.Models.ElementPositions;
using RevitSetLevelSection.Models.LevelProviders;
using RevitSetLevelSection.Models.Repositories;
using RevitSetLevelSection.ViewModels;
using RevitSetLevelSection.Views;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitSetLevelSection {
    [Transaction(TransactionMode.Manual)]
    public class SetLevelSectionCommand : BasePluginCommand {
        public SetLevelSectionCommand() {
            PluginName = "Назначение уровня/секции";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = new StandardKernel()) {
                kernel.Bind<UIApplication>()
                    .ToConstant(uiApplication)
                    .InTransientScope();
                kernel.Bind<Application>()
                    .ToConstant(uiApplication.Application)
                    .InTransientScope();

                kernel.Bind<RevitRepository>().ToSelf()
                    .InSingletonScope();

                kernel.Bind<ILevelRepository>()
                    .ToMethod(c => c.Kernel.Get<RevitRepository>()).InSingletonScope();

                kernel.Bind<FillMassParam>().ToSelf();
                kernel.Bind<FillLevelParam>().ToSelf();

                kernel.Bind<IFillParamFactory>()
                    .To<FillParamFactory>().InSingletonScope();

                kernel.Bind<IFillAdskParamFactory>()
                    .To<FillAdskParamFactory>().InSingletonScope();
                
                kernel.Bind<IBimModelPartsService>()
                    .ToMethod(c => GetPlatformService<IBimModelPartsService>());

                kernel.Bind<BasePoint>()
                    .ToMethod(c => c.Kernel.Get<RevitRepository>().GetBasePoint());

                kernel.Bind<ElementPositionFactory>().ToSelf().InSingletonScope();
                
                kernel.Bind<ARLevelProviderFactory>().ToSelf().InSingletonScope();
                kernel.Bind<KRLevelProviderFactory>().ToSelf().InSingletonScope();
                kernel.Bind<VISLevelProviderFactory>().ToSelf().InSingletonScope();

                kernel.Bind<ElementBottomPosition>().ToSelf().InSingletonScope();
                kernel.Bind<ElementMiddlePosition>().ToSelf().InSingletonScope();
                kernel.Bind<ElementTopPosition>().ToSelf().InSingletonScope();

                kernel.Bind<LevelBottomProvider>().ToSelf();
                kernel.Bind<LevelByIdProvider>().ToSelf();
                kernel.Bind<LevelMagicBottomProvider>().ToSelf();
                kernel.Bind<LevelNearestProvider>().ToSelf();
                kernel.Bind<LevelStairsProvider>().ToSelf();
                kernel.Bind<LevelTopProvider>().ToSelf();

                kernel.Bind<ILevelProviderFactoryFactory>()
                    .To<LevelProviderFactoryFactory>().InSingletonScope();

                kernel.Bind<DesignOptionAdapt>().ToSelf();
                kernel.Bind<DefaultDesignOption>().ToSelf();

                kernel.Bind<LinkTypeViewModel>().ToSelf();
                kernel.Bind<DesignOptionsViewModel>().ToSelf();

                kernel.Bind<IViewModelFactory>()
                    .To<ViewModelFactory>().InSingletonScope();
                kernel.Bind<IDesignOptionFactory>()
                    .To<DesignOptionFactory>().InSingletonScope();

                kernel.Bind<MainViewModel>().ToSelf()
                    .InSingletonScope();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<MainViewModel>());

                UpdateParams(uiApplication);

                CheckLevels();
                Check(kernel);
                ShowDialog(kernel);
            }
        }

        private static void UpdateParams(UIApplication uiApplication) {
            ProjectParameters projectParameters = ProjectParameters.Create(uiApplication.Application);
            projectParameters.SetupRevitParams(uiApplication.ActiveUIDocument.Document,
                SharedParamsConfig.Instance.BuildingWorksLevel,
                SharedParamsConfig.Instance.BuildingWorksBlock,
                SharedParamsConfig.Instance.BuildingWorksSection,
                SharedParamsConfig.Instance.BuildingWorksTyping,
                SharedParamsConfig.Instance.FixBuildingWorks);
        }

        private void Check(IKernel kernel) {
            var revitRepository = kernel.Get<RevitRepository>();
            if(revitRepository.IsKoordFile()) {
                TaskDialog.Show(PluginName,
                    $"Данный скрипт не работает в координационном файле.");
                throw new OperationCanceledException();
            }
        }
        
        private void CheckLevels() {
            var service = GetPlatformService<IPlatformCommandsService>();
            string message = null;
            Guid commandId = PlatformCommandIds.CheckLevelsCommandId;
            Result commandResult = service.InvokeCommand(commandId, ref message, new ElementSet());
            if(commandResult == Result.Failed) {
                TaskDialog.Show(PluginName, message);
                throw new OperationCanceledException();
            }
        }

        private void ShowDialog(IKernel kernel) {
            MainWindow window = kernel.Get<MainWindow>();
            if(window.ShowDialog() == true) {
                GetPlatformService<INotificationService>()
                    .CreateNotification(PluginName, "Выполнение скрипта завершено успешно.", "C#")
                    .ShowAsync();
            } else {
                GetPlatformService<INotificationService>()
                    .CreateWarningNotification(PluginName, "Выполнение скрипта отменено.")
                    .ShowAsync();
            }
        }
    }
}
