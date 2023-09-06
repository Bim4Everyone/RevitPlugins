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

using RevitPluginTemplate.Models;
using RevitPluginTemplate.ViewModels;
using RevitPluginTemplate.Views;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitPluginTemplate {
    [Transaction(TransactionMode.Manual)]
    public class RevitPluginTemplateCommand : BasePluginCommand {
        public RevitPluginTemplateCommand() {
            PluginName = "RevitPluginTemplate";
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
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<MainViewModel>());
				
				Notification(kernel.Get<MainWindow>());
			}
        }
    }
}