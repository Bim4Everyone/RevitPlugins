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

            if(elementInWall.Host == null || !(elementInWall.Host is Wall wall))
                throw new ArgumentNullException(nameof(elementInWall), "Элемент не находится в стене.");

            lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelThickness, wall.Width);
        }
    }
}

