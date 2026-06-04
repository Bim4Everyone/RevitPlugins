using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.Templates;

namespace RevitRoundingOfAreas.Models;

internal class CheckProjectParams(SystemPluginConfig systemPluginConfig, Application application, Document document) {
    private readonly ProjectParameters _projectParameters = ProjectParameters.Create(application);
    private readonly bool _isChecked = true;

    /// <summary>
    /// Метод проверки загрузки параметров
    /// </summary>
    public bool GetIsChecked() {
        return _isChecked;
    }

    /// <summary>
    /// Метод копирования параметров проекта
    /// </summary>
    public CheckProjectParams CopyProjectParams() {
        _projectParameters.SetupRevitParams(document, systemPluginConfig.RoomAreaParam);
        return this;
    }
}
