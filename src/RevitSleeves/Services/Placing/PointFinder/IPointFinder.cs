using Autodesk.Revit.DB;

namespace RevitSleeves.Services.Placing.PointFinder;
internal interface IPointFinder<T> where T : class {
    XYZ GetPoint(T param);
}
