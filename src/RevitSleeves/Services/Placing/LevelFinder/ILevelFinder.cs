using Autodesk.Revit.DB;

namespace RevitSleeves.Services.Placing.LevelFinder;
internal interface ILevelFinder<T> where T : class {
    Level GetLevel(T param);
}
