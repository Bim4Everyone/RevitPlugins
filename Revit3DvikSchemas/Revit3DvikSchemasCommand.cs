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

using Revit3DvikSchemas.Models;
using Revit3DvikSchemas.ViewModels;
using Revit3DvikSchemas.Views;

namespace Revit3DvikSchemas {
    [Transaction(TransactionMode.Manual)]
    public class Revit3DvikSchemasCommand : BasePluginCommand {
        
        public Revit3DvikSchemasCommand() {
            PluginName = "Revit3DvikSchemas";
        }

        protected override void Execute(UIApplication uiApplication) {

			using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
					
				kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());
				
				kernel.Bind<MainViewModel>().ToSelf();


                //����� �� ��������� �������� ��� MainWindow, ����� �������� � MainViewModel
				kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<MainViewModel>());
                
				
				Notification(kernel.Get<MainWindow>());
			}
        }
    }
}
