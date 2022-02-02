using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class NumberLintelParameterViewModel : BaseViewModel, ILintelParameterViewModel {
        private double _value;
        private string _name;

        public double Value {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public void SetTo(FamilyInstance lintel, FamilyInstance elementInWall) {
            if(lintel is null) {
                throw new ArgumentNullException(nameof(lintel));
            }

            //TODO: другие версии Revit
#if D2020 || R2020
            var value = UnitUtils.Convert(Value, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_DECIMAL_FEET);
            lintel.SetParamValue(Name, value);
#endif
            //lintel.SetParamValue(Name, Value);

        }
    }
}
