using System;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class DiagonalGetter : IParameterGetter<DoubleParamValue> {
        private readonly MepCurveWallClash _clash;
        private readonly Plane _plane;
        private readonly MepCategory _categoryOptions;

        public DiagonalGetter(MepCurveWallClash clash, Plane plane, MepCategory categoryOptions) {
            _clash = clash;
            _plane = plane;
            _categoryOptions = categoryOptions;
        }

        public ParameterValuePair<DoubleParamValue> GetParamValue() {
            var height = _clash.Curve.GetHeight();
            var width = _clash.Curve.GetWidth();
            var coordinateSystem = _clash.Curve.GetConnectorCoordinateSystem();
            var dirX = coordinateSystem.BasisX;
            var dirY = coordinateSystem.BasisY;

            height += _categoryOptions.GetOffset(height);
            width += _categoryOptions.GetOffset(width);
            var diagonals = new[] { _plane.ProjectVector(dirX * width + dirY * height).GetLength(), 
                                    _plane.ProjectVector(dirX * width - dirY * height).GetLength() };

            return new ParameterValuePair<DoubleParamValue>() {
                ParamName = RevitRepository.OpeningDiameter,
                TValue = new DoubleParamValue(diagonals.Max())
            };
        }
    }
}
