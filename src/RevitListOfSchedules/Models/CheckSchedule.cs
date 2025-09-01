using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.Schedules;
using dosymep.Bim4Everyone.Templates;

namespace RevitListOfSchedules.Models;
internal class CheckSchedule {
    private readonly Application _application;
    private readonly UIDocument _activeUIDocument;
    private readonly ProjectParameters _projectParameters;
    private readonly RevitScheduleRule _revitScheduleRule;

    public CheckSchedule(Application application, UIDocument activeUIDocument) {
        _application = application;
        _activeUIDocument = activeUIDocument;
        _projectParameters = ProjectParameters.Create(_application);
        _revitScheduleRule = GetScheduleRule();
    }

    public bool IsActiveViewSchedule() {
        return _revitScheduleRule.ScheduleName.Equals(_activeUIDocument.ActiveView.Name);
    }

    public bool ReplaceSchedule() {
        return !IsActiveViewSchedule() && _projectParameters.SetupSchedule(_activeUIDocument.Document, true, _revitScheduleRule);
    }

    private static RevitScheduleRule GetScheduleRule() {
        return SchedulesConfig.Instance.ListOfSchedules;
    }
}
