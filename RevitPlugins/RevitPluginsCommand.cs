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

namespace RevitPlugins {
    [Transaction(TransactionMode.Manual)]
    public class RevitPluginsCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            Console.WriteLine($"RevitVersion_{commandData.Application.Application.VersionNumber}");
            return Result.Succeeded;
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