using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitUnmodelingMep.Models;

internal class EditorChecker {
    private readonly Document _doc;
    private readonly List<string> _editedReports = [];
    private string _editedStatusReport;
    private string _syncStatusReport;
    public string FinalReport;

    public EditorChecker(Document doc) {
        _doc = doc;
    }

    private bool IsElementEdited(Element element) {
        var updateStatus = WorksharingUtils.GetModelUpdatesStatus(_doc, element.Id);

        if(updateStatus == ModelUpdatesStatus.UpdatedInCentral
           || updateStatus == ModelUpdatesStatus.DeletedInCentral) {
            _syncStatusReport =
                "Вы владеете элементами, но ваш файл устарел. Выполните синхронизацию.";
        }

        string name = GetElementEditorName(element);
        if(name != null && !_editedReports.Contains(name)) {
            _editedReports.Add(name);
        }

        if(name != null || updateStatus == ModelUpdatesStatus.UpdatedInCentral) {
            return true;
        }

        return false;
    }

    public void GetReport(Element element) {

        if(IsElementEdited(element)) {
            if(_editedReports.Count > 0) {
                _editedStatusReport =
                    "Сведения о проекте заняты пользователем: "
                    + string.Join(", ", _editedReports);
            }

            FinalReport = _editedStatusReport ?? _syncStatusReport;
        }
    }

    private string GetElementEditorName(Element element) {
        // имя текущего пользователя Revit
        string userName = _doc.Application.Username;

        string editedBy =
            (string) element.GetParamValueOrDefault(BuiltInParameter.EDITED_BY);

        if(string.IsNullOrEmpty(editedBy)) {
            return null;
        }

        if(string.Equals(
            editedBy,
            userName,
            StringComparison.OrdinalIgnoreCase)) {
            return null;
        }

        return editedBy;
    }
}

