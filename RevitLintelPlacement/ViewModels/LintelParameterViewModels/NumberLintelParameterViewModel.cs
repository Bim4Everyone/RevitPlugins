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
            lintel.SetParamValue(Name, Value);
        }
    }
}
