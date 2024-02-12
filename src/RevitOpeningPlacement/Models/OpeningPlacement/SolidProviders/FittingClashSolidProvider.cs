using System;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.SolidProviders {
    internal class FittingClashSolidProvider<T> : ISolidProvider where T : Element {
        private readonly FittingClash<T> _clash;
        private readonly IAngleFinder _angleFinder;

        public FittingClashSolidProvider(FittingClash<T> clash, IAngleFinder angleFinder) {
            _clash = clash;
            _angleFinder = angleFinder;
        }

        public Solid GetSolid() {
            return _clash.GetRotatedIntersection(_angleFinder);
        }

        public BoundingBoxXYZ GetTransformedBBoxXYZ() {
            throw new NotImplementedException();
        }
    }
}
