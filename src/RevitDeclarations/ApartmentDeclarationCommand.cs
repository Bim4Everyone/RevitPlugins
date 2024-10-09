using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitDeclarations.Models;
using RevitDeclarations.ViewModels;
using RevitDeclarations.Views;

namespace RevitDeclarations {
    [Transaction(TransactionMode.Manual)]
    public class ApartmentDeclarationCommand : BasePluginCommand {
        public ApartmentDeclarationCommand() {
            PluginName = "Декларации";
        }

        protected override void Execute(UIApplication uiApplication) {
			using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
					
				kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());
				
				kernel.Bind<ApartMainVM>().ToSelf();
				kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<ApartMainVM>());
				
				Notification(kernel.Get<MainWindow>());
			}
        }
    }
}
