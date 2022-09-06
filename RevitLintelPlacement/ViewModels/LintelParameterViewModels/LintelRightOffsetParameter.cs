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
    internal class LintelRightOffsetParameter : BaseViewModel, ILintelParameterViewModel {
        private readonly RevitRepository _revitRepository;
        private double _rightOffset;

        public LintelRightOffsetParameter(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
        }

        public double RightOffset {
            get => _rightOffset;
            set => this.RaiseAndSetIfChanged(ref _rightOffset, value);
        }
#if REVIT_2020_OR_LESS
        public double RightOffsetInternal => UnitUtils.ConvertToInternalUnits(RightOffset, DisplayUnitType.DUT_MILLIMETERS);
#else
        public double RightOffsetInternal => UnitUtils.ConvertToInternalUnits(RightOffset, UnitTypeId.Millimeters);
#endif

        public void SetTo(FamilyInstance lintel, FamilyInstance elementInWall) {
            lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelRightOffset, RightOffsetInternal);
        }
    }
}
