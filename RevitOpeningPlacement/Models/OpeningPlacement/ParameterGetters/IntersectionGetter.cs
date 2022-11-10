
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Extensions;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class IntersectionGetter<T> where T : Element {
        private readonly MepCurveClash<T> _clash;

        public IntersectionGetter(MepCurveClash<T> clash) {
            _clash = clash;
        }

        public Solid GetIntersection() {
            return BooleanOperationsUtils.ExecuteBooleanOperation(_clash.Curve.GetSolid(),
                SolidUtils.CreateTransformed(_clash.Element.GetSolid(), _clash.ElementTransform),
                BooleanOperationsType.Intersect);
        }
    }
}
