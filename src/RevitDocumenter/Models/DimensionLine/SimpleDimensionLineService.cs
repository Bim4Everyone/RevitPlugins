using System;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.DimensionLine;
internal class SimpleDimensionLineService : IDimensionLineService {
    /// <summary>
    /// Создает линию единичной длины перпендикулярно передаваемому направлению
    /// </summary>
    public Line GetPerpendicularLine(XYZ point, XYZ direction) {
        return Line.CreateBound(point, point + direction.CrossProduct(XYZ.BasisZ));
    }

    public Line GetDimensionLine(RebarElement rebar, XYZ direction) {
        return rebar.Rebar.Location is not LocationPoint pt
            ? throw new ArgumentException(nameof(rebar))
            : GetPerpendicularLine(pt.Point, direction);
    }
}
