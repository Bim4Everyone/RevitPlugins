using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectParams;

using dosymep.Bim4Everyone.Templates;

namespace RevitListOfSchedules.Models;
internal class CheckProjectParams {
    private readonly Application _application;
    private readonly Document _document;
    private readonly ProjectParameters _projectParameters;
    private readonly bool _isChecked = true;

    public CheckProjectParams(Application application, Document document) {
        _application = application;
        _document = document;
        _projectParameters = ProjectParameters.Create(_application);
    }

    public bool GetIsChecked() {
        return _isChecked;
    }

    public CheckProjectParams CopyProjectParams() {
        _projectParameters.SetupRevitParams(_document,
                ProjectParamsConfig.Instance.ListOfSchedulesSheetName,
                ProjectParamsConfig.Instance.ListOfSchedulesRevNumber,
                ProjectParamsConfig.Instance.ListOfSchedulesNotes,
                ProjectParamsConfig.Instance.ListOfSchedulesGroup,
                ProjectParamsConfig.Instance.ListOfSchedulesListName);
        return this;
    }
}
