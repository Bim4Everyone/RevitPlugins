#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

#endregion

namespace RevitCopyStandarts {
    public class CopyStandartsApp : IExternalApplication {
        public Result OnStartup(UIControlledApplication application) {
            try {
                application.CreateRibbonTab("BIM 1");
            } catch {

            }

            RibbonPanel ribbonPanel = application.CreateRibbonPanel("BIM 1", "Стандартизация");
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            var commandButton = new PushButtonData(
                "CopyStandartsCommand",
                "Копирование стандартов",
                thisAssemblyPath,
                "RevitCopyStandarts.CopyStandartsCommand");

            //commandButton.Image = new BitmapImage(new Uri("pack://application:,,,/RevitRibbonAddin;component/Resources/image.png"));
            commandButton.ToolTip = "Копирование" + Environment.NewLine + "стандартов";

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application) {
            return Result.Succeeded;
        }
    }
}
