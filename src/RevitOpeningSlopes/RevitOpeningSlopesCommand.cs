using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitOpeningSlopes.Models;
using RevitOpeningSlopes.Services;
using RevitOpeningSlopes.ViewModels;
using RevitOpeningSlopes.Views;

namespace RevitOpeningSlopes {
    [Transaction(TransactionMode.Manual)]
    public class RevitOpeningSlopesCommand : BasePluginCommand {
        public RevitOpeningSlopesCommand() {
            PluginName = "RevitOpeningSlopes";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<LinesFromOpening>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<SlopesDataGetter>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<CreationOpeningSlopes>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<SlopeParams>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());
                kernel.Bind<AlreadySelectedWindowsGetter>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<ManuallySelectedWindowsGetter>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<OnActiveViewWindowsGetter>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<NearestElements>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<SolidOperations>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<MainViewModel>().ToSelf();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<MainViewModel>());

                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
