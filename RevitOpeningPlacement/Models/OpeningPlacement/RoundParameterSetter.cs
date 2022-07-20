using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class RoundParameterSetter : IParameterSetter {
        private readonly double _diameter;
        private readonly double _width;
        private List<Offset> _offsets;

        public RoundParameterSetter(double diameter, double width, List<Offset> offsets) {
            _diameter = diameter;
            _width = width;
            _offsets = offsets;
        }
        public void SetParameters(FamilyInstance familyInstance) {
            _offsets = _offsets.Select(item => new Offset() {
                From = ConvertMmToFt(item.From),
                To = ConvertMmToFt(item.To),
                OffsetValue = ConvertMmToFt(item.OffsetValue)
            }).ToList();

            var offset = _offsets.FirstOrDefault(item => item.From <= _diameter && item.To >= _diameter)?.OffsetValue;

            if(offset == null) {
                offset = 0;
            }

            if(familyInstance.IsExistsParam(RevitRepository.OpeningDiameter)) {
                familyInstance.SetParamValue(RevitRepository.OpeningDiameter, _diameter + offset.Value);
            }

            if(familyInstance.IsExistsParam(RevitRepository.OpeningThickness)) {
                familyInstance.SetParamValue(RevitRepository.OpeningThickness, _width);
            }
        }

        private double ConvertMmToFt(double value) {
#if R2020 || D2020
            return UnitUtils.Convert(value, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_DECIMAL_FEET);
#else
            return UnitUtils.Convert(value, UnitTypeId.Millimeters, UnitTypeId.Feet);
#endif
        }
    }
}
