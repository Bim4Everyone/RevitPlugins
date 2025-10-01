using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;
internal class WallOpeningsGroupPointFinder : RoundValueGetter, IPointFinder {
    private readonly OpeningsGroup _group;
    /// <summary>
    /// Округление высотной отметки отверстия в мм
    /// </summary>
    private readonly int _heightRound;

    /// <summary>
    /// Конструирует провайдер точки вставки для объединенного задания на отверстие
    /// </summary>
    /// <param name="group">Объединенное задание на отверстие</param>
    /// <param name="heightRounding">Округление отметки в мм</param>
    public WallOpeningsGroupPointFinder(OpeningsGroup group, int heightRounding) {
        _group = group ?? throw new System.ArgumentNullException(nameof(group));
        _heightRound = heightRounding;
    }

    public XYZ GetPoint() {
        var transform = _group.GetTransform();
        var bb = _group.Elements.Select(item => SolidUtils.CreateTransformed(item.GetSolid(), transform.Inverse))
            .Select(item => item.GetTransformedBoundingBox())
            .ToList()
            .CreateUnitedBoundingBox();
        var center = bb.Min + (bb.Max - bb.Min) / 2;
        double zCoordinate = _group.IsCylinder ? center.Z : bb.Min.Z;
        var transformedPoint = _group.GetTransform().OfPoint(new XYZ(center.X, bb.Min.Y, zCoordinate));
        return new XYZ(transformedPoint.X, transformedPoint.Y, RoundFeetToMillimeters(transformedPoint.Z, _heightRound));
    }
}
