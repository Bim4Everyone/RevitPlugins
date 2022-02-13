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

        public void SetTo(FamilyInstance lintel, FamilyInstance elementInWall) {
#if D2020 || R2020
            var value = UnitUtils.Convert(RightOffset, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_DECIMAL_FEET);
            lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelRightOffset, value); //ToDo: параметр
#endif
        }
    }
}
