using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using DevExpress.XtraScheduler.Outlook.Interop;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using Ninject;

using pyRevitLabs.Json.Serialization;

using RevitCheckingLevels.Models;
using RevitCheckingLevels.Services;
using RevitCheckingLevels.ViewModels;
using RevitCheckingLevels.Views;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitCheckingLevels {
    [Transaction(TransactionMode.Manual)]
    public class RevitCheckingLevelsCommand : BasePluginCommand {
        public RevitCheckingLevelsCommand() {
            PluginName = "Проверка уровней";
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

                kernel.Bind<MainViewModel>()
                    .ToSelf()
                    .InSingletonScope();
                
                kernel.Bind<CheckingLevelConfig>()
                    .ToMethod(c => CheckingLevelConfig.GetCheckingLevelConfig());

                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<MainViewModel>());

                if(!FromGui) {
                    var mainViewModel = kernel.Get<MainViewModel>();
                    mainViewModel.ViewLoadCommand.Execute(null);
                    if(!mainViewModel.HasErrors) {
                        return;
                    }
                }

                Window mainWindow = kernel.Get<MainWindow>();
                bool? dialogResult = mainWindow.ShowDialog();

                if(!FromGui) {
                    var mainViewModel = kernel.Get<MainViewModel>();
                    if(mainViewModel.HasErrors) {
                        throw new InvalidOperationException("Были обнаружены ошибки в уровнях.");
                    }
                }

                Notification(dialogResult);
            }
        }
    }
}