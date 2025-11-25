using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitListOfSchedules.Models;
internal class InstancesAssembly {
    // Ключевое слово для поиска среди строк в заголовке спецификации
    private readonly IEnumerable<string> _approvedLines = ["Ведомость", "Спецификация"];
    private readonly RevitRepository _revitRepository;
    private readonly ViewDrafting _viewDrafting;
    private readonly FamilySymbol _familySymbol;
    private readonly string _albumName;

    public InstancesAssembly(
        RevitRepository revitRepository,
        ViewDrafting viewDrafting,
        FamilySymbol familySymbol,
        string albumName) {
        _revitRepository = revitRepository;
        _viewDrafting = viewDrafting;
        _familySymbol = familySymbol;
        _albumName = albumName;
    }

    public void PlaceFamilyInstances(string sheetNumber, string sheetRevNumber, IList<ViewSchedule> listOfSchedules) {
        foreach(var schedule in listOfSchedules) {
            var tableData = schedule.GetTableData();
            var headData = tableData.GetSectionData(SectionType.Header);

            if(headData != null && !headData.HideSection) {
                string resultScheduleName = null;
                bool found = false;

                for(int i = 0; i < headData.NumberOfRows && !found; i++) {
                    for(int j = 0; j < headData.NumberOfColumns && !found; j++) {
                        string cellText = headData.GetCellText(i, j);
                        if(_approvedLines.Any(cellText.Contains)) {
                            resultScheduleName = cellText;
                            found = true;
                        }
                    }
                }
                if(found) {
                    PlaceFamilyInstance(sheetNumber, sheetRevNumber, resultScheduleName);
                } else {
                    PlaceFamilyInstance(sheetNumber, sheetRevNumber, schedule.Name);
                }
            }
        }
    }

    public void PlaceFamilyInstance(string sheetNumber, string sheetRevNumber, string scheduleName) {
        var familyInstance = _revitRepository.Document.Create.NewFamilyInstance(XYZ.Zero, _familySymbol, _viewDrafting);
        familyInstance.SetParamValue(ParamFactory.ListOfSchedulesSheetName, sheetNumber);
        familyInstance.SetParamValue(ParamFactory.ListOfSchedulesRevNumber, sheetRevNumber);
        familyInstance.SetParamValue(ParamFactory.ListOfSchedulesListName, scheduleName);
        familyInstance.SetParamValue(ParamFactory.ListOfSchedulesGroup, $"{ParamFactory.DefaultScheduleName}_{_albumName}");
    }

    public void DeleteFamilyInstances() {
        var instances = new FilteredElementCollector(_revitRepository.Document, _viewDrafting.Id)
            .OfType<FamilyInstance>()
            .Select(instance => instance.Id)
            .ToList();
        if(instances.Any()) {
            _revitRepository.Document.Delete(instances);
        }
    }
}
