using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Dimensions.DimensionLines;
internal interface IDimensionLineProvider<TContext> where TContext : class {
    Line GetDimensionLine(TContext context);
}
