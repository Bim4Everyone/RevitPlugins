using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;

namespace RevitVORChecker.Models;

/// <summary>
/// Доступ к приложению Revit.
/// </summary>
/// <remarks>
/// Проверяемые модели открываются из папки в фоне, поэтому активный документ не используется.
/// </remarks>
internal class RevitRepository {
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    public UIApplication UIApplication { get; }

    public Application Application => UIApplication.Application;
}
