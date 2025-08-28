using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.Schedules;
using dosymep.Bim4Everyone.Templates;

namespace RevitListOfSchedules.Models;
internal class CheckSchedule {
    private readonly UIApplication _uiApplication;
    private readonly ProjectParameters _projectParameters;
    private readonly RevitScheduleRule _revitScheduleRule;

    public CheckSchedule(UIApplication uiApplication) {
        _uiApplication = uiApplication;
        _projectParameters = ProjectParameters.Create(_uiApplication.Application);
        _revitScheduleRule = GetScheduleRule();
    }

    public bool IsActiveViewSchedule() {
        return _revitScheduleRule.ScheduleName.Equals(_uiApplication.ActiveUIDocument.ActiveView.Name);
    }

    public bool ReplaceSchedule() {
        return !IsActiveViewSchedule() && _projectParameters.SetupSchedule(_uiApplication.ActiveUIDocument.Document, true, _revitScheduleRule);
    }

    private static RevitScheduleRule GetScheduleRule() {
        return SchedulesConfig.Instance.ListOfSchedules;
    }
}
