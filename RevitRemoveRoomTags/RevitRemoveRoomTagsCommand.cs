using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using Ninject;

using RevitRemoveRoomTags.Models;
using RevitRemoveRoomTags.ViewModels;
using RevitRemoveRoomTags.Views;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitRemoveRoomTags {
    [Transaction(TransactionMode.Manual)]
    public class RevitRemoveRoomTagsCommand : BasePluginCommand {
        public RevitRemoveRoomTagsCommand() {
            PluginName = "Удалить марки помещений";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = new StandardKernel()) {
                kernel.Bind<UIApplication>()
                    .ToConstant(uiApplication)
                    .InTransientScope();
                kernel.Bind<Application>()
                    .ToConstant(uiApplication.Application)
                    .InTransientScope();

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
