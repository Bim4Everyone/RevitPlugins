using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
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

                kernel.Bind<IWallCreationDataProvider>()
                    .To<WallCreationDataProvider>()
                    .InSingletonScope();
                kernel.Bind<IRoomFinisher>()
                    .To<RoomFinisher>()
                    .InSingletonScope();
                kernel.Bind<IWallCreatorFactory>()
                    .To<WallCreatorFactory>()
                    .InSingletonScope();
                kernel.Bind<UnconnectedTopWallCreator>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<TopConnectedToLevelWallCreator>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<TopConnectedToRoomTopWallCreator>()
                    .ToSelf()
                    .InSingletonScope();


                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

                kernel.UseWpfUIThemeUpdater();

                kernel.BindMainWindow<MainViewModel, MainWindow>();
                kernel.UseWpfUIProgressDialog<MainViewModel>();

                kernel.Bind<RichErrorMessageService>()
                    .ToSelf()
                    .InTransientScope();
                kernel.Bind<ErrorWindowViewModel>()
                    .ToSelf()
                    .InTransientScope();
                kernel.Bind<ErrorWindow>()
                    .ToSelf()
                    .InTransientScope();
                kernel.UseWpfUIMessageBox<ErrorWindowViewModel>();

                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                kernel.UseWpfLocalization(
                    $"/{assemblyName};component/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));

                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
