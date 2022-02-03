using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels.LintelParameterViewModels {
    internal class LintelLeftOffsetParameter : BaseViewModel, ILintelParameterViewModel {
        private double _leftOffset;

        public double LeftOffset { 
            get => _leftOffset;
            set => this.RaiseAndSetIfChanged(ref _leftOffset, value); 
        }

        public void SetTo(FamilyInstance lintel, FamilyInstance elementInWall) {
#if D2020 || R2020
            var value = UnitUtils.Convert(LeftOffset, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_DECIMAL_FEET);
            lintel.SetParamValue("ОпираниеСлева", value); //ToDo: параметр
#endif
        }
    }
}
