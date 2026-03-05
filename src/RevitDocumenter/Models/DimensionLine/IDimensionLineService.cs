using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.DimensionLine;
internal interface IDimensionLineService {
    Line GetLine(RebarElement rebar, XYZ direction);
}
