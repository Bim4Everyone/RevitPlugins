using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitListOfSchedules.Models;
internal class InstancesAssembly {
    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _revitRepository;
    private readonly FamilyLoadOptions _familyLoadOptions;
    private readonly string _albumName;
    private readonly ViewDrafting _viewDrafting;
    private readonly FamilySymbol _familySymbol;


    public InstancesAssembly(
        ILocalizationService localizationService,
        RevitRepository revitRepository,
        FamilyLoadOptions familyLoadOptions,
        string albumName) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
        _familyLoadOptions = familyLoadOptions;
        _albumName = albumName;
        _viewDrafting = _revitRepository.GetViewDrafting(_albumName);

        var tempDoc = new TempFamilyDocument(_localizationService, _revitRepository, _familyLoadOptions, albumName);
        _familySymbol = tempDoc.FamilySymbol;

        DeleteFamilyInstances(_viewDrafting);
    }

    public void PlaceFamilyInstances(string sheetNumber, string sheetRevNumber, IList<ViewSchedule> listOfSchedules) {
        foreach(var schedule in listOfSchedules) {
            var tableData = schedule.GetTableData();
            var headData = tableData.GetSectionData(SectionType.Header);
            string scheduleName = headData == null
                ? schedule.Name
                : headData.GetCellText(0, 0);
            PlaceFamilyInstance(sheetNumber, sheetRevNumber, scheduleName);
        }
    }

    public void PlaceFamilyInstance(string sheetNumber, string sheetRevNumber, string scheduleName) {
        var familyInstance = _revitRepository.Document.Create.NewFamilyInstance(XYZ.Zero, _familySymbol, _viewDrafting);
        familyInstance.SetParamValue(ParamFactory.ListOfSchedulesSheetName, sheetNumber);
        familyInstance.SetParamValue(ParamFactory.ListOfSchedulesRevNumber, sheetRevNumber);
        familyInstance.SetParamValue(ParamFactory.ListOfSchedulesListName, scheduleName);
        familyInstance.SetParamValue(ParamFactory.ListOfSchedulesGroup, $"{ParamFactory.DefaultScheduleName}_{_albumName}");
    }

    // Метод удаления экземпляров семейства с вида
    private void DeleteFamilyInstances(View view) {
        var instances = new FilteredElementCollector(_revitRepository.Document, view.Id)
            .OfType<FamilyInstance>()
            .Select(instance => instance.Id)
            .ToList();
        if(instances.Any()) {
            _revitRepository.Document.Delete(instances);
        }
    }
}
