using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels.LintelParameterViewModels {
    internal class OpeningWidthParameter : ILintelParameterViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly ElementInfosViewModel _elementInfos;

        public OpeningWidthParameter(RevitRepository revitRepository, ElementInfosViewModel elementInfos) {
            this._revitRepository = revitRepository;
            this._elementInfos = elementInfos;
        }

        public void SetTo(FamilyInstance lintel, FamilyInstance elementInWall) {
            if(lintel is null) {
                throw new ArgumentNullException(nameof(lintel));
            }

            if(elementInWall is null) {
                throw new ArgumentNullException(nameof(elementInWall));
            }
            var elementWidth = elementInWall.GetParamValueOrDefault(_revitRepository.LintelsCommonConfig.OpeningWidth) ??
                               elementInWall.Symbol.GetParamValueOrDefault(BuiltInParameter.FAMILY_WIDTH_PARAM);

            if(elementWidth == null) {
                _elementInfos.ElementInfos.Add(new ElementInfoViewModel(elementInWall.Id,
                    InfoElement.MissingOpeningParameter.FormatMessage(elementInWall.Name, _revitRepository.LintelsCommonConfig.OpeningWidth)));
                _elementInfos.ElementInfos.Add(new ElementInfoViewModel(lintel.Id,
                    InfoElement.UnsetLintelParamter.FormatMessage(lintel.Name, _revitRepository.LintelsCommonConfig.LintelWidth)));
            } else {
                lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelWidth, (double) elementWidth);
            }
        }
    }
}
