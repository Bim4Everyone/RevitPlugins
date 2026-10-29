using System.Collections.Generic;
using System.Linq;

namespace RevitDocumenter.Models.RoomDimensions.Contours;

/// <summary>
/// Замкнутая петля контура помещения. Элементы идут по порядку обхода.
/// </summary>
internal class ContourLoop {
    public int Index { get; set; }

    /// <summary>
    /// Внешний контур помещения (иначе внутренний: колонна, пилон).
    /// </summary>
    public bool IsOuter { get; set; }

    /// <summary>
    /// Ориентированная площадь петли (кв. футы). Положительная при обходе против часовой стрелки.
    /// </summary>
    public double SignedArea { get; set; }

    public List<ContourElement> Elements { get; set; } = [];

    /// <summary>
    /// Количество линий петли до нормализации.
    /// </summary>
    public int RawLineCount { get; set; }

    /// <summary>
    /// Действия нормализации (для журнала).
    /// </summary>
    public List<string> NormalizationLog { get; } = [];

    public IEnumerable<ContourLine> Lines => Elements.OfType<ContourLine>();
    public IEnumerable<ContourPoint> Points => Elements.OfType<ContourPoint>();

    /// <summary>
    /// Предыдущий элемент по обходу (петля замкнута).
    /// </summary>
    public ContourElement GetPrevious(int position) {
        int count = Elements.Count;
        return Elements[(position - 1 + count) % count];
    }

    /// <summary>
    /// Следующий элемент по обходу (петля замкнута).
    /// </summary>
    public ContourElement GetNext(int position) {
        return Elements[(position + 1) % Elements.Count];
    }
}
