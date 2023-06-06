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

using RevitFamilyParameterAdder.Models;
using RevitFamilyParameterAdder.ViewModels;
using RevitFamilyParameterAdder.Views;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitFamilyParameterAdder {
    [Transaction(TransactionMode.Manual)]
    public class RevitFamilyParameterAdderCommand : BasePluginCommand {
        public RevitFamilyParameterAdderCommand() {
            PluginName = "Добавление параметров в семейство";
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

                Check(kernel);

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

        private void Check(IKernel kernel) {
            var revitRepositiry = kernel.Get<RevitRepository>();
            if(!revitRepositiry.IsFamilyFile()) {
                TaskDialog.Show(PluginName,
                    $"Данный скрипт работает только в файле семейства.");
                throw new OperationCanceledException();
            }
        }
    }
}