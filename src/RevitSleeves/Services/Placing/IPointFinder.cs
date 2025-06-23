using Autodesk.Revit.DB;

namespace RevitSleeves.Services.Placing;
internal interface IPointFinder<T> where T : class {
    XYZ GetPoint(T param);
}
