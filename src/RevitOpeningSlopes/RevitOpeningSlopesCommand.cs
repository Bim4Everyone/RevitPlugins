using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitOpeningSlopes.Models;
using RevitOpeningSlopes.Services.ValueGetter;
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
                kernel.Bind<OpeningTopXYZGetter>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<OpeningCenterXYZGetter>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<OpeningRightXYZGetter>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<OpeningFrontPointGetter>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<OpeningHeightGetter>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<OpeningWidthGetter>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());

                kernel.Bind<MainViewModel>().ToSelf();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<MainViewModel>());

                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
