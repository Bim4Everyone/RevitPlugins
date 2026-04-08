using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitListOfSchedules.Models;
internal class InstancesAssembly {
    private readonly ILocalizationService _localizationService;
    private readonly IList<string> _approvedLines;
    private readonly RevitRepository _revitRepository;

    public InstancesAssembly(
        ILocalizationService localizationService,
        RevitRepository revitRepository) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
        string localizedApprovedLines = _localizationService.GetLocalizedString("InstancesAssembly.ApprovedLines");
        _approvedLines = localizedApprovedLines
            .Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
    }

    public void PlaceFamilyInstances(
        string sheetNumber,
        string sheetRevNumber,
        IList<ViewSchedule> listOfSchedules,
        FamilySymbol familySymbol,
        ViewDrafting viewDrafting,
        string albumName) {
        foreach(var schedule in listOfSchedules) {
            var tableData = schedule.GetTableData();
            var headData = tableData.GetSectionData(SectionType.Header);

            if(headData != null && !headData.HideSection) {
                string resultScheduleName = null;
                bool found = false;

                for(int i = headData.FirstRowNumber; i < headData.NumberOfRows && !found; i++) {
                    for(int j = headData.FirstColumnNumber; j < headData.NumberOfColumns && !found; j++) {
                        string cellText = headData.GetCellText(i, j);
                        if(_approvedLines.Any(cellText.ToLower().Contains)) {
                            resultScheduleName = cellText;
                            found = true;
                        }
                    }
                }
                if(found) {
                    PlaceFamilyInstance(sheetNumber, sheetRevNumber, resultScheduleName, familySymbol, viewDrafting, albumName);
                } else if(!found
                    && headData.NumberOfRows == 1
                    && headData.NumberOfColumns == 1
                    && string.IsNullOrEmpty(headData.GetCellText(0, 0))) {

                    PlaceFamilyInstance(sheetNumber, sheetRevNumber, schedule.Name, familySymbol, viewDrafting, albumName);
                }
            }
        }
    }

    public void PlaceFamilyInstance(
        string sheetNumber,
        string sheetRevNumber,
        string scheduleName,
        FamilySymbol familySymbol,
        ViewDrafting viewDrafting,
        string albumName) {
        var familyInstance = _revitRepository.Document.Create.NewFamilyInstance(XYZ.Zero, familySymbol, viewDrafting);
        familyInstance.SetParamValue(ParamFactory.ListOfSchedulesSheetName, sheetNumber);
        familyInstance.SetParamValue(ParamFactory.ListOfSchedulesRevNumber, sheetRevNumber);
        familyInstance.SetParamValue(ParamFactory.ListOfSchedulesListName, scheduleName);
        familyInstance.SetParamValue(ParamFactory.ListOfSchedulesGroup, $"{ParamFactory.DefaultScheduleName}_{albumName}");
    }

    public void DeleteFamilyInstances(ViewDrafting viewDrafting) {
        var instances = new FilteredElementCollector(_revitRepository.Document, viewDrafting.Id)
            .OfType<FamilyInstance>()
            .Select(instance => instance.Id)
            .ToList();
        if(instances.Any()) {
            _revitRepository.Document.Delete(instances);
        }
    }
}
