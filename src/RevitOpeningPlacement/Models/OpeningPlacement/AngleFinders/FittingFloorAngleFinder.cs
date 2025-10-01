using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
/// <summary>
/// Находит угол поворота для заданий на отверстия по фитингам в перекрытиях
/// </summary>
internal class FittingFloorAngleFinder : IAngleFinder {
    private readonly FamilyInstance _fitting;

    public FittingFloorAngleFinder(FamilyInstance fitting) {
        _fitting = fitting ?? throw new System.ArgumentNullException(nameof(fitting));
    }


    public Rotates GetAngle() {
        var horizConnector = _fitting.GetConnectors()?
            .FirstOrDefault(c => c.Shape == ConnectorProfileType.Rectangular
                && (c.CoordinateSystem.BasisZ.IsAlmostEqualTo(XYZ.BasisZ)
                || c.CoordinateSystem.BasisZ.Negate().IsAlmostEqualTo(XYZ.BasisZ)));
        return horizConnector != null
            ? new Rotates(0, 0, XYZ.BasisX.AngleOnPlaneTo(horizConnector.CoordinateSystem.BasisX, XYZ.BasisZ))
            : new Rotates(0, 0, 0);
    }
}
