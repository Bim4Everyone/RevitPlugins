using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitWindowGapPlacement.ViewModels;
using RevitWindowGapPlacement.Views;

namespace RevitWindowGapPlacement {
    [Transaction(TransactionMode.Manual)]
    public class SettingsWindowGapCommand : BasePluginCommand {
        public SettingsWindowGapCommand() {
            PluginName = "Настройки модуля проемов окон";
        }

        protected override void Execute(UIApplication uiApplication) {
            var window = new MainWindow() {
                DataContext = new MainViewModel() {
                    CurrentViewModel = new SettingsViewModel() {WindowTitle = PluginName}
                }
            };
            
            var helper = new WindowInteropHelper(window) {Owner = uiApplication.MainWindowHandle};

            window.ShowDialog();
        }
    }
}