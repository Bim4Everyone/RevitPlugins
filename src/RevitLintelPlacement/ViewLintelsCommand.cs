using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels;
using RevitLintelPlacement.Views;

namespace RevitLintelPlacement {

    [Transaction(TransactionMode.Manual)]
    public class ViewLintelsCommand : BasePluginCommand {
        public ViewLintelsCommand() {
            PluginName = "Расстановщик перемычек";
        }
        
        protected override void Execute(UIApplication uiApplication) {
            var lintelsConfig = LintelsConfig.GetLintelsConfig();
            var revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document, lintelsConfig);
            if(!HasConfig(revitRepository.LintelsCommonConfig)) {
                return;
            }
            var elementInfos = new ElementInfosViewModel(revitRepository);
            var lintelsView = new LintelCollectionViewModel(revitRepository, elementInfos);
            var view = new LintelsView() { DataContext = lintelsView};
            view.Show();
        }

        private bool HasConfig(LintelsCommonConfig lintelsConfig) {
            if(lintelsConfig.IsEmpty()) {
                TaskDialog.Show("BIM", "Необходимо заполнить настройки плагина");
                return false;
            }
            return true;
        }
    }
}
