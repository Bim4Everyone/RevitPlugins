using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitCreateViewSheet.Models;
using RevitCreateViewSheet.Services;
using RevitCreateViewSheet.ViewModels;
using RevitCreateViewSheet.Views;

namespace RevitCreateViewSheet {
    [Transaction(TransactionMode.Manual)]
    public class CreateViewSheetCommand : BasePluginCommand {
        public CreateViewSheetCommand() {
            PluginName = "Менеджер листов";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();

                AddEntitiesServices(kernel);

                kernel.UseWpfUIThemeUpdater();

                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));
                kernel.BindMainWindow<MainViewModel, MainWindow>();

                kernel.UseWpfSaveFileDialog<MainViewModel>(filter: "JSON (*.json)|*.json");
                kernel.UseWpfOpenFileDialog<MainViewModel>(filter: "JSON (*.json)|*.json");
                kernel.UseWpfUIMessageBox<MainViewModel>();

                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                kernel.UseWpfLocalization(
                    $"/{assemblyName};component/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));
                kernel.UseWpfUIProgressDialog<MainViewModel>();

                Notification(kernel.Get<MainWindow>());
            }
        }

        private void AddEntitiesServices(IKernel kernel) {
            kernel.Bind<EntitiesHandler>()
                .ToSelf()
                .InSingletonScope();
            kernel.Bind<NewEntitySaver>()
                .ToSelf()
                .InSingletonScope();
            kernel.Bind<ExistsEntitySaver>()
                .ToSelf()
                .InSingletonScope();
            kernel.Bind<EntitySaverProvider>()
                .ToSelf()
                .InSingletonScope();
            kernel.Bind<EntitiesTracker>()
                .ToSelf()
                .InSingletonScope();

            AddEntitiesCreationViews(kernel);
        }

        private void AddEntitiesCreationViews(IKernel kernel) {
            kernel.Bind<SheetItemsFactory>()
                .ToSelf()
                .InSingletonScope();
            kernel.Bind<AnnotationModelCreatorViewModel>()
                .ToSelf()
                .InSingletonScope();
            kernel.Bind<AnnotationModelCreatorWindow>()
                .ToSelf()
                .InTransientScope()
                .WithPropertyValue(
                    nameof(Window.DataContext),
                    c => c.Kernel.Get<AnnotationModelCreatorViewModel>());
            kernel.Bind<ScheduleModelCreatorViewModel>()
                .ToSelf()
                .InSingletonScope();
            kernel.Bind<ScheduleModelCreatorWindow>()
                .ToSelf()
                .InTransientScope()
                .WithPropertyValue(
                    nameof(Window.DataContext),
                    c => c.Kernel.Get<ScheduleModelCreatorViewModel>());
            kernel.Bind<ViewPortModelCreatorViewModel>()
                .ToSelf()
                .InSingletonScope();
            kernel.Bind<ViewPortModelCreatorWindow>()
                .ToSelf()
                .InTransientScope()
                .WithPropertyValue(
                    nameof(Window.DataContext),
                    c => c.Kernel.Get<ViewPortModelCreatorViewModel>());
        }
    }
}
