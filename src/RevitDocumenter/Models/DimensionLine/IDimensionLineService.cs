using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.DimensionLine;
internal interface IDimensionLineService {
    Line GetDimensionLine(RebarElement rebar, XYZ direction);
    Line GetPerpendicularLine(XYZ point, XYZ direction);
}
