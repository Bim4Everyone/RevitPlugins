using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.ReferenceCollectors;
internal interface IReferenceCollector<TContext> where TContext : class {
    ReferenceArray CollectReferences(TContext context);
}
