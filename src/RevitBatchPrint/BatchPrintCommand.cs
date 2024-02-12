#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitBatchPrint.ViewModels;
using RevitBatchPrint.Views;

#endregion

namespace RevitBatchPrint {
    [Transaction(TransactionMode.Manual)]
    public class BatchPrintCommand : BasePluginCommand {
        public BatchPrintCommand() {
            PluginName = "Пакетная печать";
        }
        
        protected override void Execute(UIApplication uiApplication) {
            var window = new BatchPrintWindow() { DataContext = new PrintAbumsViewModel(uiApplication) };
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
