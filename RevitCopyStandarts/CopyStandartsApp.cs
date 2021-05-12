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
            application.CreateRibbonTab("BIM 2");
            
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("BIM 2", "��������������");
            var commandButton = new PushButtonData(
                "CopyStandartsRevitCommand",
                "�����������" + Environment.NewLine + "����������",
                Assembly.GetExecutingAssembly().Location,
                "RevitCopyStandarts.CopyStandartsRevitCommand") {
                ToolTip = "�����������" + Environment.NewLine + "����������",
                //Image = new BitmapImage(new Uri("pack://application:,,,/RevitRibbonAddin;component/Resources/image.png"))
            };

            ribbonPanel.AddItem(commandButton);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application) {
            return Result.Succeeded;
        }
    }
}
