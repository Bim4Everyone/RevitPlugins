using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.DimensionServices;
internal interface IDimensionLineProvider<TContext> where TContext : class {
    Line GetDimensionLine(TContext context);
}
