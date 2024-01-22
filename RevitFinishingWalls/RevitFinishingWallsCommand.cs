using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitFinishingWalls.Models;
using RevitFinishingWalls.Services.Creation;
using RevitFinishingWalls.Services.Creation.Implements;
using RevitFinishingWalls.ViewModels;
using RevitFinishingWalls.Views;

namespace RevitFinishingWalls {
    [Transaction(TransactionMode.Manual)]
    public class RevitFinishingWallsCommand : BasePluginCommand {
        public RevitFinishingWallsCommand() {
            PluginName = "RevitFinishingWalls";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<IRoomFinisher>().To<RoomFinisher>().InSingletonScope();
                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());

                kernel.UseXtraMessageBox<MainViewModel>();

                kernel.Bind<MainViewModel>().ToSelf();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<MainViewModel>());

                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
