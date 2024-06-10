using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitReinforcementCoefficient.Models;
using RevitReinforcementCoefficient.ViewModels;
using RevitReinforcementCoefficient.Views;

namespace RevitReinforcementCoefficient {
    [Transaction(TransactionMode.Manual)]
    public class RevitReinforcementCoefficientCommand : BasePluginCommand {
        public RevitReinforcementCoefficientCommand() {
            PluginName = "Подсчет коэффициентов армирования";
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
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<MainViewModel>());

                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
