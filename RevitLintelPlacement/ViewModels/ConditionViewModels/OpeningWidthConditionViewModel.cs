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

            var elementWidth =
                elementInWall.GetParamValueOrDefault<double?>(_revitRepository.LintelsCommonConfig.OpeningWidth)
                ?? elementInWall.Symbol.GetParamValueOrDefault<double?>(_revitRepository.LintelsCommonConfig.OpeningWidth);

            if(elementWidth == null) {
                _elementInfos.ElementInfos.Add(new ElementInfoViewModel(elementInWall.Id,
                    InfoElement.MissingOpeningParameter.FormatMessage(_revitRepository.LintelsCommonConfig.OpeningWidth)) {
                    Name = elementInWall.Name,
                    LevelName = elementInWall.LevelId != null ? _revitRepository.GetElementById(elementInWall.LevelId)?.Name : null
                });
                return false;
            }

#if REVIT_2020_OR_LESS
            double openingWidth = UnitUtils.ConvertFromInternalUnits((double) elementWidth, DisplayUnitType.DUT_MILLIMETERS);
#elif REVIT_2021
            double openingWidth = UnitUtils.ConvertFromInternalUnits((double) elementWidth, UnitTypeId.Millimeters);
#else
            double openingWidth = UnitUtils.ConvertFromInternalUnits( (double) elementWidth, UnitTypeId.Millimeters);
#endif
            return MinWidth <= openingWidth && openingWidth <= MaxWidth;
        }
    }
}
