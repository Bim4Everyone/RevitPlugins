using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitPlugins.ViewModels;
using RevitPlugins.Views;

namespace RevitPlugins {
    [Transaction(TransactionMode.Manual)]
    public class RevitPluginsCommand : BasePluginCommand {
        public RevitPluginsCommand() {
            PluginName = "RevitPluginsCommand";
        }

        protected override void Execute(UIApplication uiApplication) {
            var window = new MainWindow() {
                Title = PluginName, 
                DataContext = new MainViewModel(uiApplication)
            };

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

    [Transaction(TransactionMode.Manual)]
    public class RevitPluginsCommand1 : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            Console.WriteLine($"RevitVersion_{commandData.Application.Application.VersionNumber}");
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class RevitPluginsCommand2 : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            Console.WriteLine($"RevitVersion_{commandData.Application.Application.VersionNumber}");
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class RevitPluginsApplication : IExternalApplication {
        public Result OnStartup(UIControlledApplication application) {
            Console.WriteLine($"OnStartup: RevitVersion_{application.ControlledApplication.VersionNumber}");
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application) {
            Console.WriteLine($"OnShutdown: RevitVersion_{application.ControlledApplication.VersionNumber}");
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class RevitPluginsApplication1 : IExternalApplication {
        public Result OnStartup(UIControlledApplication application) {
            Console.WriteLine($"OnStartup: RevitVersion_{application.ControlledApplication.VersionNumber}");
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application) {
            Console.WriteLine($"OnShutdown: RevitVersion_{application.ControlledApplication.VersionNumber}");
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class RevitPluginsApplicationDB : IExternalDBApplication {
        public ExternalDBApplicationResult OnStartup(ControlledApplication application) {
            Console.WriteLine($"OnStartup: RevitVersion_{application.VersionNumber}");
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnShutdown(ControlledApplication application) {
            Console.WriteLine($"OnShutdown: RevitVersion_{application.VersionNumber}");
            return ExternalDBApplicationResult.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class RevitPluginsApplicationDB1 : IExternalDBApplication {
        public ExternalDBApplicationResult OnStartup(ControlledApplication application) {
            Console.WriteLine($"OnStartup: RevitVersion_{application.VersionNumber}");
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnShutdown(ControlledApplication application) {
            Console.WriteLine($"OnShutdown: RevitVersion_{application.VersionNumber}");
            return ExternalDBApplicationResult.Succeeded;
        }
    }
}