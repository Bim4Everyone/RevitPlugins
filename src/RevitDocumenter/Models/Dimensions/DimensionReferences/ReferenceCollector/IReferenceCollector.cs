using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Dimensions.DimensionReferences.ReferenceCollector;
internal interface IReferenceCollector<TContext> where TContext : class, IReferenceCollectorContext {
    ReferenceArray CollectReferences(TContext context);
}
