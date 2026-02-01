using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.Templates;

namespace RevitBuildCoordVolumes.Models;
internal class CheckProjectParams {
    private readonly Application _application;
    private readonly Document _document;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly ProjectParameters _projectParameters;
    private readonly bool _isChecked = true;

    public CheckProjectParams(Application application, Document document, SystemPluginConfig systemPluginConfig) {
        _application = application;
        _document = document;
        _systemPluginConfig = systemPluginConfig;
        _projectParameters = ProjectParameters.Create(_application);
    }

    /// <summary>
    /// Метод проверки загрузки параметров
    /// </summary>
    public bool GetIsChecked() {
        return _isChecked;
    }

    /// <summary>
    /// Метод копирования общих параметров проекта
    /// </summary>
    public CheckProjectParams CopyProjectParams() {
        _projectParameters.SetupRevitParams(_document, _systemPluginConfig.GetAllParameters());
        return this;
    }
}
