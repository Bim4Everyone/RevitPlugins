using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels.LintelParameterViewModels {
    internal class WallHalfThicknessParameter : ILintelParameterViewModel {
        private readonly RevitRepository _revitRepository;

        public WallHalfThicknessParameter(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
        }

        public void SetTo(FamilyInstance lintel, FamilyInstance elementInWall) {
            if(lintel is null) {
                throw new ArgumentNullException(nameof(lintel));
            }

            if(elementInWall is null) {
                throw new ArgumentNullException(nameof(elementInWall));
            }
            var elementWidth = elementInWall.GetParamValueOrDefault(_revitRepository.LintelsCommonConfig.OpeningWidth) ?? 
                               elementInWall.GetParamValueOrDefault(BuiltInParameter.FAMILY_WIDTH_PARAM)??
                               elementInWall.Symbol.GetParamValueOrDefault(BuiltInParameter.FAMILY_WIDTH_PARAM);

            if(elementWidth == null)
                throw new ArgumentException(nameof(elementInWall), $"У элемента {elementInWall.Id} отсутствует параметр \"ADSK_Размер_Ширина\".");

            lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelWidth, (double) elementWidth); //Todo: параметр
        }
    }
}

