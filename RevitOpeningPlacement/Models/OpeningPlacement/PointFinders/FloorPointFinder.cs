
using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PointFinders {
    internal class FloorPointFinder<T> : IPointFinder where T : Element {
        private readonly Clash<T, CeilingAndFloor> _clash;

        public FloorPointFinder(Clash<T, CeilingAndFloor> clash) {
            _clash = clash;
        }

        public XYZ GetPoint() {
            var solid = _clash.GetIntersection();
            var maxZ = solid.GetOutline().MaximumPoint.Z;
            var point = solid.ComputeCentroid();
            return new XYZ(point.X, point.Y, maxZ);
        }
    }
}
