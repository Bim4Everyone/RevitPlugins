using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;
using dosymep.Revit;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class OpeningWidthConditionViewModel : BaseViewModel, IConditionViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly ElementInfosViewModel _elementInfos;
        private double _minWidth;
        private double _maxWidth;

        public OpeningWidthConditionViewModel(RevitRepository revitRepository, ElementInfosViewModel elementInfos) {
            this._revitRepository = revitRepository;
            this._elementInfos = elementInfos;
        }

        public double MinWidth {
            get => _minWidth;
            set => this.RaiseAndSetIfChanged(ref _minWidth, value);
        }

        public double MaxWidth {
            get => _maxWidth;
            set => this.RaiseAndSetIfChanged(ref _maxWidth, value);
        }

        public bool Check(FamilyInstance elementInWall) {
            if(elementInWall == null || elementInWall.Id == ElementId.InvalidElementId)
                throw new ArgumentNullException(nameof(elementInWall));

            var elementWidth = elementInWall.GetParamValueOrDefault(_revitRepository.LintelsCommonConfig.OpeningWidth);
            if(elementWidth == null) {
                elementWidth = elementInWall.Symbol.GetParamValueOrDefault(_revitRepository.LintelsCommonConfig.OpeningWidth);
            }
            if(elementWidth == null) {
                elementWidth = elementInWall.Symbol.GetParamValueOrDefault(BuiltInParameter.FAMILY_WIDTH_PARAM);
            }
            if(elementWidth == null) {
                _elementInfos.ElementInfos.Add(new ElementInfoViewModel(elementInWall.Id,
                    InfoElement.MissingOpeningParameter.FormatMessage(_revitRepository.LintelsCommonConfig.OpeningWidth)) {
                    Name = elementInWall.Name,
                    LevelName = elementInWall.LevelId != null ? _revitRepository.GetElementById(elementInWall.LevelId)?.Name : null
                });
                return false;
            }

            double openingWidth;
#if D2020 || R2020
            openingWidth = UnitUtils.ConvertFromInternalUnits((double) elementWidth, DisplayUnitType.DUT_MILLIMETERS);
#elif D2021 || R2021
            openingWidth = UnitUtils.ConvertFromInternalUnits((double) elementWidth, UnitTypeId.Millimeters);

#else
            openingWidth = UnitUtils.ConvertFromInternalUnits(
                (double) elementWidth, UnitTypeId.Millimeters);
#endif
            return MinWidth <= openingWidth && openingWidth <= MaxWidth;
        }
    }
}
