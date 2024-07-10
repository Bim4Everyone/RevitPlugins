using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitClashDetective.Models;
using RevitClashDetective.ViewModels.Navigator;
using RevitClashDetective.Views;

namespace RevitClashDetective {
    [Transaction(TransactionMode.Manual)]
    public class GetClashesCommand : BasePluginCommand {
        public GetClashesCommand() {
            PluginName = "Поиск коллизий";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<ReportsViewModel>()
                    .ToSelf()
                    .InSingletonScope()
                    .WithConstructorArgument("selectedFile", (string) null);
                kernel.Bind<NavigatorView>()
                    .ToSelf()
                    .InSingletonScope()
                    .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<ReportsViewModel>());

                Notification(kernel.Get<NavigatorView>());
            }
        }
    }
}
