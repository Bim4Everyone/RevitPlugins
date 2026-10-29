using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB.Architecture;

using RevitDocumenter.Models.RoomDimensions.Settings;

namespace RevitDocumenter.Models.RoomDimensions.Contours;

/// <summary>
/// Контур помещения: внешняя петля и внутренние петли.
/// </summary>
internal class RoomContour {
    public RoomContour(Room room, RoomTypeSettings roomType) {
        Room = room;
        RoomType = roomType;
    }

    public Room Room { get; }
    public RoomTypeSettings RoomType { get; }

    /// <summary>
    /// Фактически использованная граница помещения.
    /// </summary>
    public string BoundaryLocation { get; set; }

    public List<ContourLoop> Loops { get; } = [];

    public ContourLoop Outer => Loops.FirstOrDefault(l => l.IsOuter);
    public IEnumerable<ContourLoop> Inners => Loops.Where(l => !l.IsOuter);

    /// <summary>
    /// Ошибка построения контура. Если задана, помещение пропускается.
    /// </summary>
    public string Error { get; set; }
}
