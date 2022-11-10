
using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PointFinders {
    internal class FloorPointFinder : IPointFinder {
        private readonly MepCurveClash<CeilingAndFloor> _clash;

        public FloorPointFinder(MepCurveClash<CeilingAndFloor> clash) {
            _clash = clash;
        }

        public XYZ GetPoint() {
            var solid = new IntersectionGetter<CeilingAndFloor>(_clash).GetIntersection();
            var maxZ = solid.GetOutline().MaximumPoint.Z;
            var point = solid.ComputeCentroid();
            return new XYZ(point.X, point.Y, maxZ);
        }

    }
}
