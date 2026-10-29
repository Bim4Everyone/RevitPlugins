using System.Collections.Generic;
using System.Linq;

using RevitDocumenter.Models.RoomDimensions.Contours;
using RevitDocumenter.Models.RoomDimensions.Planning;

namespace RevitDocumenter.Models.RoomDimensions;

/// <summary>
/// Итог запуска образмеривания на виде.
/// </summary>
internal class RoomDimensioningResult {
    public int DeletedDimensionCount { get; set; }
    public int UnmatchedRoomCount { get; set; }
    public List<RoomRunResult> Rooms { get; } = [];

    /// <summary>
    /// Скрытые предупреждения Revit.
    /// </summary>
    public List<string> Warnings { get; set; } = [];

    /// <summary>
    /// Количество отладочных линий контура.
    /// </summary>
    public int DebugLineCount { get; set; }

    public string ImagePath { get; set; }
    public string ImageError { get; set; }

    public string DumpPath { get; set; }
    public string DumpError { get; set; }

    public int RoomErrorCount => Rooms.Count(r => r.Contour.Error != null);
    public int LineCount => Rooms.Sum(r => r.Contour.Loops.Sum(l => l.Lines.Count()));
    public int LinesWithoutReferenceCount => Rooms.Sum(r => r.Contour.Loops.Sum(l => l.Lines.Count(x => x.Reference is null)));
    public int DimensionCount => Rooms.Sum(r => r.Dimensions.Count);
    public int DimensionOkCount => Rooms.Sum(r => r.Dimensions.Count(d => d.Status == "Ok"));
}

/// <summary>
/// Итог по одному помещению.
/// </summary>
internal class RoomRunResult {
    public RoomRunResult(RoomContour contour) {
        Contour = contour;
    }

    public RoomContour Contour { get; }
    /// <summary>
    /// Размеры по контурам после удаления дублей и объединения в цепочки.
    /// </summary>
    public List<VirtualDimension> Dimensions { get; set; } = [];

    /// <summary>
    /// Журнал планирования размеров.
    /// </summary>
    public List<string> PlanningLog { get; } = [];
}
