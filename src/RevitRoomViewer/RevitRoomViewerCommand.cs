using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Views;

using Ninject;

using RevitRoomViewer.Models;
using RevitRoomViewer.ViewModels;
using RevitRoomViewer.Views;

namespace RevitRoomViewer {
    [Transaction(TransactionMode.Manual)]
    public class RevitRoomViewerCommand : BasePluginCommand {
        public RevitRoomViewerCommand() {
            PluginName = "RevitRoomViewer";
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
                    .WithPropertyValue(nameof(PlatformWindow.LocalizationService),
                        c => c.Kernel.Get<ILocalizationService>());

                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
