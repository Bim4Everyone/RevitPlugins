using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitParamValuesByEvents.Models;
using RevitParamValuesByEvents.ViewModels;
using RevitParamValuesByEvents.Views;

namespace RevitParamValuesByEvents {
    [Transaction(TransactionMode.Manual)]
    public class RevitParamValuesByEventsCommand : BasePluginCommand {
        public RevitParamValuesByEventsCommand() {
            PluginName = "RevitParamValuesByEvents";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());

                kernel.Bind<SettingsPageVM>().ToSelf();
                kernel.Bind<SettingsPage>().ToSelf()
                                .WithPropertyValue(nameof(FrameworkElement.DataContext), c => c.Kernel.Get<SettingsPageVM>());

                SettingsPage page = kernel.Get<SettingsPage>();
            }
        }
    }
}
