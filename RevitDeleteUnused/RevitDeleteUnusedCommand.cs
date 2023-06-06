using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using Ninject;

using RevitDeleteUnused.Models;
using RevitDeleteUnused.ViewModels;
using RevitDeleteUnused.Views;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitDeleteUnused {
    [Transaction(TransactionMode.Manual)]
    public class RevitDeleteUnusedCommand : BasePluginCommand {
        public RevitDeleteUnusedCommand() {
            PluginName = "Удалить неиспользуемые";
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

                MainWindow window = kernel.Get<MainWindow>();
                if(window.ShowDialog() == true) {
                    GetPlatformService<INotificationService>()
                        .CreateNotification(PluginName, "Выполнение скрипта завершено успешно.", "C#")
                        .ShowAsync();
                } else {
                    GetPlatformService<INotificationService>()
                        .CreateWarningNotification(PluginName, "Выполнение скрипта отменено.")
                        .ShowAsync();
                }
            }
        }
    }
}