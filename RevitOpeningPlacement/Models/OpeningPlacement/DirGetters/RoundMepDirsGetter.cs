using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.DirGetters {
    internal class RoundMepDirsGetter : IDirectionsGetter {
        private readonly MepCurveWallClash _clash;
        private readonly Plane _plane;

        public RoundMepDirsGetter(MepCurveWallClash clash, Plane plane) {
            _clash = clash;
            _plane = plane;
        }

        public IEnumerable<XYZ> GetDirections() {
            var transformedMepLine = _clash.GetTransformedMepLine();

            var angle = _plane.GetAngleOnPlaneToYAxis(transformedMepLine.Direction);
            XYZ dir;
            if(Math.Abs(Math.Cos(angle)) < 0.0001) {
                dir = _plane.YVec;
            } else {
                var projectedDir = _plane.ProjectVector(transformedMepLine.Direction);

                var vector = (projectedDir.GetLength() / Math.Cos(angle)) * _plane.YVec;
                dir = (vector - projectedDir).Normalize();
            }

            yield return dir;
            yield return -dir;
        }
    }
}
