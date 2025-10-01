using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;
internal class FloorOpeningsGroupPointFinder : IPointFinder {
    private readonly OpeningsGroup _group;

    public FloorOpeningsGroupPointFinder(OpeningsGroup group) {
        _group = group;
    }

    public XYZ GetPoint() {
        // получаем трансформацию от начала проекта первого задания
        var transform = _group.GetTransform();
        // находим бокс, ограничивающий все задания из группы в координатах относительно первого задания
        var bb = _group.Elements
            .Select(item => SolidUtils
                .CreateTransformed(item.GetSolid(), transform.Inverse)
                .GetTransformedBoundingBox())
            .ToList()
            .CreateUnitedBoundingBox();
        var center = bb.Min + (bb.Max - bb.Min) / 2;
        // возвращаем центр верхней грани бокса, но трансформированный в изначальное положение заданий в проекте
        return transform.OfPoint(new XYZ(center.X, center.Y, bb.Max.Z));
    }
}
