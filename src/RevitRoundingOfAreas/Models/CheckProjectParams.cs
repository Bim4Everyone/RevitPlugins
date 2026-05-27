using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.Templates;

namespace RevitRoundingOfAreas.Models;
internal class CheckProjectParams {
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly Application _application;
    private readonly Document _document;
    private readonly ProjectParameters _projectParameters;
    private readonly bool _isChecked = true;

    public CheckProjectParams(SystemPluginConfig systemPluginConfig, Application application, Document document) {
        _systemPluginConfig = systemPluginConfig;
        _application = application;
        _document = document;
        _projectParameters = ProjectParameters.Create(_application);
    }

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
        _projectParameters.SetupRevitParams(_document, _systemPluginConfig.RoomAreaParam);
        return this;
    }
}
