using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class RoundParamterGetter : IParameterGetter {
        private readonly MEPCurve _curve;
        private readonly Wall _wall;

        public RoundParamterGetter(MEPCurve curve, Wall wall) {
            _curve = curve;
            _wall = wall;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            if(_curve.IsPerpendicular(_wall)) {
                return GetPerpendicularCurveValues();
            } else {
                return GetInclinedCurveValues();
            }
        }

        private IEnumerable<ParameterValuePair> GetPerpendicularCurveValues() {
            yield return new ParameterValuePair() {
                ParamName = RevitRepository.OpeningDiameter,
                Value = new DoubleParamValue(_curve.GetDiameter())
            };

            yield return new ParameterValuePair() {
                ParamName = RevitRepository.OpeningThickness,
                Value = new DoubleParamValue(_wall.Width)
            };
        }

        private IEnumerable<ParameterValuePair> GetInclinedCurveValues() {
            var mepLine = (Line) ((LocationCurve) _curve.Location).Curve;
            var faces = GetFaces();

            var elongatedMepLine = Line.CreateBound(mepLine.GetEndPoint(0) - mepLine.Direction * _wall.Width / 2,
                                                  mepLine.GetEndPoint(1) + mepLine.Direction * _wall.Width / 2);


            throw new NotImplementedException();
        }

        private IEnumerable<Face> GetFaces() {
            var interiorFace = HostObjectUtils.GetSideFaces(_wall, ShellLayerType.Interior);
            var exteriorFace = HostObjectUtils.GetSideFaces(_wall, ShellLayerType.Exterior);

            yield return (Face) _wall.GetGeometryObjectFromReference(interiorFace[0]);
            yield return (Face) _wall.GetGeometryObjectFromReference(exteriorFace[0]);
        }
    }
}
