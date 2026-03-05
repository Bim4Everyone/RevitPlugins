using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Comparision;
internal interface IComparisonService {
    ReferenceArray Compare(IComparisonContext context);
}
