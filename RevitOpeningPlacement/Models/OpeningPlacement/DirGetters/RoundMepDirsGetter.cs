using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.DirGetters {
    internal class RoundMepDirsGetter : IDirectionsGetter {
        private readonly MepCurveWallClash _clash;
        private readonly IProjector _projector;

        public RoundMepDirsGetter(MepCurveWallClash clash, IProjector projector) {
            _clash = clash;
            _projector = projector;
        }
        public IEnumerable<XYZ> GetDirections() {
            var transformedMepLine = _clash.GetTransformedMepLine();

            var angle = _projector.GetAngleOnPlaneToAxis(transformedMepLine.Direction);
            XYZ dir;
            if(Math.Abs(Math.Cos(angle)) < 0.0001) {
                dir = _projector.GetPlaneY();
            } else {
                var projectedDir = _projector.ProjectVector(transformedMepLine.Direction);

                var vector = (projectedDir.GetLength() / Math.Cos(angle)) * _projector.GetPlaneY();
                dir = (vector - projectedDir).Normalize();
            }

            yield return dir;
            yield return -dir;
        }
    }
}
