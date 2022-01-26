using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class OpeningWidthConditionViewModel : BaseViewModel, IConditionViewModel {

        private double _minWidth;
        private double _maxWidth;

        public OpeningWidthConditionViewModel() {

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
                throw new ArgumentNullException("На проверку не передан элемент.");

            double openingWidth;
#if D2020 || R2020
            openingWidth = UnitUtils.ConvertFromInternalUnits(elementInWall.Symbol.LookupParameter("Ширина").AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
#else
            openingWidth = UnitUtils.ConvertFromInternalUnits(elementInWall.Symbol.LookupParameter("Ширина").AsDouble(), UnitTypeId.Millimeters);
#endif
            return MinWidth <= openingWidth && openingWidth < MaxWidth;
        }
    }
}
