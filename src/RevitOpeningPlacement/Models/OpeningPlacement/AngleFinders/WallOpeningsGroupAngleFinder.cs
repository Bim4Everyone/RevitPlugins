using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
internal class WallOpeningsGroupAngleFinder : IAngleFinder {
    private readonly OpeningsGroup _openingsGroup;

    public WallOpeningsGroupAngleFinder(OpeningsGroup openingsGroup) {
        _openingsGroup = openingsGroup;
    }

    public Rotates GetAngle() {
        var transform = _openingsGroup.GetTransform();
        double angle = XYZ.BasisY.AngleTo(transform.BasisY);
        return (transform.BasisY.X <= 0 && transform.BasisY.Y <= 0) || (transform.BasisY.X <= 0 && transform.BasisY.Y >= 0) ? new Rotates(0, 0, angle) : new Rotates(0, 0, -angle);
    }
}
