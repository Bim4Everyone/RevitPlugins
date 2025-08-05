using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.ProjectParams;

using dosymep.Bim4Everyone.Templates;

namespace RevitListOfSchedules.Models;

internal class CheckProjectParams {
    private readonly UIApplication _uiApplication;
    private readonly ProjectParameters _projectParameters;

    private readonly bool _isChecked = true;

    public CheckProjectParams(UIApplication uiApplication) {
        _uiApplication = uiApplication;
        _projectParameters = ProjectParameters.Create(_uiApplication.Application);
    }

    public bool GetIsChecked() {
        return _isChecked;
    }

    public CheckProjectParams CopyProjectParams() {
        _projectParameters.SetupRevitParams(_uiApplication.ActiveUIDocument.Document,
                ProjectParamsConfig.Instance.IsRoomNumberFix,
                ProjectParamsConfig.Instance.IsRoomLevelFix,
                ProjectParamsConfig.Instance.NumberingOrder,
                ProjectParamsConfig.Instance.IsRoomMainLevel,
                ProjectParamsConfig.Instance.RoomGroupName);
        return this;
    }
}
