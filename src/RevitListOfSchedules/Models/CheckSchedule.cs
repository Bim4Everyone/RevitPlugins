using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.Schedules;
using dosymep.Bim4Everyone.Templates;

namespace RevitListOfSchedules.Models;
internal class CheckSchedule {
    private readonly UIApplication _uiApplication;
    private readonly ProjectParameters _projectParameters;

    private readonly bool _isChecked = true;

    public CheckSchedule(UIApplication uiApplication) {
        _uiApplication = uiApplication;
        _projectParameters = ProjectParameters.Create(_uiApplication.Application);
    }

    public bool GetIsChecked() {
        return _isChecked;
    }

    public CheckSchedule ReplaceSchedule() {
        _projectParameters.SetupSchedule(_uiApplication.ActiveUIDocument.Document, true, GetScheduleRule());
        return this;
    }

    private static RevitScheduleRule GetScheduleRule() {
        return SchedulesConfig.Instance.RoomsCheckGroup;
    }
}
