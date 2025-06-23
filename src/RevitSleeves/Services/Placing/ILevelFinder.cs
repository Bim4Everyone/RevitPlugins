using Autodesk.Revit.DB;

namespace RevitSleeves.Services.Placing;
internal interface ILevelFinder<T> where T : class {
    Level GetLevel(T param);
}
