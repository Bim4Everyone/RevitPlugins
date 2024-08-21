using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitAxonometryViews.Models;
using RevitAxonometryViews.ViewModels;
using RevitAxonometryViews.Views;

namespace RevitAxonometryViews {
    [Transaction(TransactionMode.Manual)]
    public class RevitAxonometryViewsCommand : BasePluginCommand {
        public RevitAxonometryViewsCommand() {
            PluginName = "RevitAxonometryViews";
        }

        protected override void Execute(UIApplication uiApplication) {
            RevitRepository repo = new RevitRepository(uiApplication);
            repo.Execute();
        }
    }
}
