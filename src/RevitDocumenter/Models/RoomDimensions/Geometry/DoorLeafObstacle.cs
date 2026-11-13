using System.Collections.Generic;

namespace RevitDocumenter.Models.RoomDimensions.Geometry;

/// <summary>
/// Выпуклый контур фактически показанного открытого полотна (D77), координаты XY в футах модели.
/// Дуга открывания и габарит всего семейства в препятствие не входят.
/// </summary>
internal class DoorLeafObstacle {
    public DoorLeafObstacle(long doorId, IReadOnlyList<Point2D> corners) {
        DoorId = doorId;
        Corners = corners;
    }

    public long DoorId { get; }
    public IReadOnlyList<Point2D> Corners { get; }
}
