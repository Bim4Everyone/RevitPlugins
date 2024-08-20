using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class FittingClash<T> : Clash<FamilyInstance, T> where T : Element {
        public FittingClash(RevitRepository revitRepository, ClashModel clashModel) : base(revitRepository, clashModel) { }
        public override double GetConnectorArea() {
            return Element1.GetMaxConnectorArea();
        }

        public Solid GetRotatedIntersection(IAngleFinder angleFinder) {
            var solid = GetIntersection();
            var zRotates = -angleFinder.GetAngle().Z;
            var transform = Transform.Identity.GetRotationMatrixAroundZ(zRotates);
            return SolidUtils.CreateTransformed(solid, transform);
        }
    }
}
