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

using RevitSectionsConstructor.Models;
using RevitSectionsConstructor.ViewModels;
using RevitSectionsConstructor.Views;

namespace RevitSectionsConstructor {
    [Transaction(TransactionMode.Manual)]
    public class RevitSectionsConstructorCommand : BasePluginCommand {
        public RevitSectionsConstructorCommand() {
            PluginName = "RevitSectionsConstructor";
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
