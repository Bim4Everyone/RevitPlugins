using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitUnmodelingMep.Models;

internal class EditorChecker {
    private readonly Document _document;
    private readonly ILocalizationService _localizationService;
    private readonly List<string> _editedReports = [];

    private string _editedStatusReport;
    private string _syncStatusReport;

    public string FinalReport { get; private set; }

    public EditorChecker(Document document, ILocalizationService localizationService) {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    public void GetReport(Element element) {
        if(IsElementEdited(element)) {
            if(_editedReports.Count > 0) {
                _editedStatusReport = _localizationService.GetLocalizedString("EditorChecker.SettigsIsBusy")
                                      + string.Join(", ", _editedReports);
            }

            FinalReport = _editedStatusReport ?? _syncStatusReport;
        }
    }

    private bool IsElementEdited(Element element) {
        var updateStatus = WorksharingUtils.GetModelUpdatesStatus(_document, element.Id);

        if(updateStatus == ModelUpdatesStatus.UpdatedInCentral
           || updateStatus == ModelUpdatesStatus.DeletedInCentral) {
            _syncStatusReport = _localizationService.GetLocalizedString("EditorChecker.SettigsIsOutDated");
        }

        string name = GetElementEditorName(element);
        if(name != null && !_editedReports.Contains(name)) {
            _editedReports.Add(name);
        }

        return name != null || updateStatus == ModelUpdatesStatus.UpdatedInCentral;
    }

    private string GetElementEditorName(Element element) {
        // Имя текущего пользователя Revit
        string userName = _document.Application.Username;

        string editedBy = (string) element.GetParamValueOrDefault(BuiltInParameter.EDITED_BY);
        if(string.IsNullOrEmpty(editedBy)) {
            return null;
        }

        if(string.Equals(editedBy, userName, StringComparison.OrdinalIgnoreCase)) {
            return null;
        }

        return editedBy;
    }
}

