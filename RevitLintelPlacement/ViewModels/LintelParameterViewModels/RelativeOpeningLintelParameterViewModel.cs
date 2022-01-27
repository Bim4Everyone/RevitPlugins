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
    internal class RelativeOpeningLintelParameterViewModel : BaseViewModel, ILintelParameterViewModel {
        private double _relationValue;
        private string _name;
        private string _openingParameterName;

        public double RelationValue {
            get => _relationValue;
            set => this.RaiseAndSetIfChanged(ref _relationValue, value);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string OpeningParameterName { 
            get => _openingParameterName;
            set => this.RaiseAndSetIfChanged(ref _openingParameterName, value);
        }

        public void SetTo(FamilyInstance lintel, FamilyInstance elementInWall) {
            if(lintel is null) {
                throw new ArgumentNullException(nameof(lintel));
            }

            if(elementInWall is null) {
                throw new ArgumentNullException(nameof(elementInWall));
            }

            var openingWidth = (double)elementInWall.Symbol.GetParamValueOrDefault(BuiltInParameter.FAMILY_WIDTH_PARAM);
            lintel.SetParamValue(Name, openingWidth * RelationValue);
        }
    }


}
