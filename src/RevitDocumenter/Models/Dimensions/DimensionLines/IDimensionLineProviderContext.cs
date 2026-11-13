using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Dimensions.DimensionLines;
internal interface IDimensionLineProviderContext {
    Element Element { get; set; }
    XYZ Direction { get; set; }
}
