using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitRoomViewer.Models;
using RevitRoomViewer.ViewModels;
using RevitRoomViewer.Views;

namespace RevitRoomViewer {
    [Transaction(TransactionMode.Manual)]
    public class RevitRoomViewerCommand : BasePluginCommand {
        public RevitRoomViewerCommand() {
            PluginName = "Просмотр помещений";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());

                kernel.Bind<MainViewModel>().ToSelf();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<MainViewModel>())
                    .WithPropertyValue(nameof(Window.Title),
                        c => PluginName);

                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
