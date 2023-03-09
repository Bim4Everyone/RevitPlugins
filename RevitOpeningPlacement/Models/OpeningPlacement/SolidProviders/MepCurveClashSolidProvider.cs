
using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.SolidProviders {
    internal class MepCurveClashSolidProvider<T> : ISolidProvider where T : Element {
        private readonly MepCurveClash<T> _clash;

        public MepCurveClashSolidProvider(MepCurveClash<T> clash) {
            _clash = clash;
        }

        public Solid GetSolid() {
            return _clash.GetIntersection();
        }

        public BoundingBoxXYZ GetTransformedBBoxXYZ() {
            throw new System.NotImplementedException();
        }
    }
}
