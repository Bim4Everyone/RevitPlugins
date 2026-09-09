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

        // Спецификации без шапки не учитываем
        if(headData == null || headData.HideSection)
            continue;

        // Шапка должна содержать хотя бы одну ячейку
        if(headData.NumberOfRows < 1 || headData.NumberOfColumns < 1)
            continue;

        string resultScheduleName = null;

        // Сначала ищем подходящее имя среди ячеек шапки
        for(int i = headData.FirstRowNumber; 
            i < headData.FirstRowNumber + headData.NumberOfRows && resultScheduleName == null; i++) {
            for(int j = headData.FirstColumnNumber; j < headData.FirstColumnNumber + headData.NumberOfColumns && resultScheduleName == null; j++) {
                string cellText = headData.GetCellText(i, j);

                if(!string.IsNullOrEmpty(cellText) && _approvedLines.Any(x => cellText.ToLower().Contains(x.ToLower()))) {
                    resultScheduleName = cellText;
                }
            }
        }

        // Если в шапке ничего подходящего нет,
        // проверяем название самой спецификации
        if(resultScheduleName == null && _approvedLines.Any(x => schedule.Name.ToLower().Contains(x.ToLower()))) {
            resultScheduleName = schedule.Name;
        }

        // Если нашли подходящее имя — размещаем
        if(resultScheduleName != null) {
            PlaceFamilyInstance(sheetNumber, sheetRevNumber, resultScheduleName, familySymbol, viewDrafting, albumName);
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
