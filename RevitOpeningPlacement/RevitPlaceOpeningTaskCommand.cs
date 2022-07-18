using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement {
    class RevitPlaceOpeningTaskCommand : BasePluginCommand {
        public RevitPlaceOpeningTaskCommand() {
            PluginName = "Настройка ";
        }

        protected override void Execute(UIApplication uiApplication) {
            RevitClashDetective.Models.RevitRepository revitRepository
                = new RevitClashDetective.Models.RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            var openingConfig = OpeningConfig.GetOpeningConfig();
            if(openingConfig.Categories.Count > 0) {

            }
        }

        private IEnumerable<Filter> GetMepFilters(RevitClashDetective.Models.RevitRepository revitRepository, IEnumerable<MepCategory> categories) {
            var minSizePipe = categories.FirstOrDefault(item => item.Name.Equals(RevitRepository.CategoryNames[MepCategoriesEnum.Pipe]))?.MinSizes?.FirstOrDefault();
            if(minSizePipe != null) {
                yield return FiltersInitializer.GetPipeFilter(revitRepository, minSizePipe.Value);
            }

            var minSizeRoundDuct = categories.FirstOrDefault(item => item.Name.Equals(RevitRepository.CategoryNames[MepCategoriesEnum.RoundDuct]))?.MinSizes?.FirstOrDefault();
            if(minSizeRoundDuct != null) {
                yield return FiltersInitializer.GetRoundDuctFilter(revitRepository, minSizeRoundDuct.Value);
            }

            var minSizesRectangleDuct = categories.FirstOrDefault(item => item.Name.Equals(RevitRepository.CategoryNames[MepCategoriesEnum.RectangleDuct]))?.MinSizes.ToList();
            if(minSizesRectangleDuct != null) {
                yield return FiltersInitializer.GetRectangleDuctFilter(revitRepository, minSizesRectangleDuct.FirstOrDefault(item=>item.Name.Eq));
            }

            var minSize = categories.FirstOrDefault(item => item.Name.Equals(RevitRepository.CategoryNames[MepCategoriesEnum.Pipe]))?.MinSizes?.FirstOrDefault();
            if(minSizePipe != null) {
                yield return FiltersInitializer.GetPipeFilter(revitRepository, minSizePipe.Value);
            }
        }
    }
}
