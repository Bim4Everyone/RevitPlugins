using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitFinishingWalls.Models;
using RevitFinishingWalls.Services;
using RevitFinishingWalls.Services.Creation;
using RevitFinishingWalls.Services.Creation.Implements;
using RevitFinishingWalls.ViewModels;
using RevitFinishingWalls.Views;

namespace RevitFinishingWalls {
    [Transaction(TransactionMode.Manual)]
    public class RevitFinishingWallsCommand : BasePluginCommand {
        public RevitFinishingWallsCommand() {
            PluginName = "Отделка стен";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<RevitEventHandler>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<IWallCreationDataProvider>().To<WallCreationDataProvider>().InSingletonScope();
                kernel.Bind<IRoomFinisher>().To<RoomFinisher>().InSingletonScope();
                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());
                kernel.Bind<ErrorWindowViewModel>().ToSelf().InTransientScope();
                kernel.Bind<ErrorWindow>().ToSelf().InTransientScope();
                kernel.Bind<RichErrorMessageService>().ToSelf().InTransientScope();

                kernel.UseWpfUIMessageBox<ErrorWindowViewModel>();

                kernel.UseWpfUIThemeUpdater();

                kernel.UseWpfUIProgressDialog<MainViewModel>();
                kernel.BindMainWindow<MainViewModel, MainWindow>();

                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                kernel.UseWpfLocalization(
                    $"/{assemblyName};component/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));

                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
