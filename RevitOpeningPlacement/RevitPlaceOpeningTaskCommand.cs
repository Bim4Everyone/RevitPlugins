using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement {

    [Transaction(TransactionMode.Manual)]
    public class RevitPlaceOpeningTaskCommand : BasePluginCommand {
        public RevitPlaceOpeningTaskCommand() {
            PluginName = "Настройка ";
        }

        protected override void Execute(UIApplication uiApplication) {
            RevitClashDetective.Models.RevitRepository revitRepository
                = new RevitClashDetective.Models.RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            var openingConfig = OpeningConfig.GetOpeningConfig();
            if(openingConfig.Categories.Count > 0) {
                var filters = GetMepFilters(revitRepository, openingConfig.Categories);
                foreach(var filter in filters) {
                    
                }
            }
        }

        private IEnumerable<Filter> GetMepFilters(RevitClashDetective.Models.RevitRepository revitRepository, MepCategoryCollection categories) {
            var minSizePipe = categories[BuiltInCategory.OST_PipeCurves]?.MinSizes[Parameters.Diameter];
            if(minSizePipe != null) {
                yield return FiltersInitializer.GetPipeFilter(revitRepository, minSizePipe.Value);
            }

            var minSizeRoundDuct = categories[BuiltInCategory.OST_DuctCurves]?.MinSizes[Parameters.Diameter];
            if(minSizeRoundDuct != null) {
                yield return FiltersInitializer.GetRoundDuctFilter(revitRepository, minSizeRoundDuct.Value);
            }

            var minSizesRectangleDuct = categories[BuiltInCategory.OST_DuctCurves]?.MinSizes;
            if(minSizesRectangleDuct != null) {
                var height = minSizesRectangleDuct[Parameters.Height];
                var width = minSizesRectangleDuct[Parameters.Width];
                if(height != null && width != null) {
                    yield return FiltersInitializer.GetRectangleDuctFilter(revitRepository, height.Value, width.Value);
                }
            }

            var minSizesTray = categories[BuiltInCategory.OST_CableTray]?.MinSizes;
            if(minSizesTray != null) {
                var height = minSizesTray[Parameters.Height];
                var width = minSizesTray[Parameters.Width];
                if(height != null && width != null) {
                    yield return FiltersInitializer.GetTrayFilter(revitRepository, height.Value, width.Value);
                }
            }
        }

        
    }
}
