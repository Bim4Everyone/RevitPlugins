using System;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.DimensionLine;
internal class SimpleDimensionLineService : IDimensionLineService {

    public Line GetLine(RebarElement rebar, XYZ direction) {
        return rebar.Rebar.Location is not LocationPoint pt
            ? throw new ArgumentException(nameof(rebar))
            : Line.CreateBound(pt.Point, pt.Point + rebar.Rebar.FacingOrientation.CrossProduct(XYZ.BasisZ));
    }
}
