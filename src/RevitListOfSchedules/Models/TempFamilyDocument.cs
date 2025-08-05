using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitListOfSchedules.Interfaces;


namespace RevitListOfSchedules.Models;
internal class TempFamilyDocument : IFamilyDocument {
    private const string _extension = ".rfa";
    private const int _diameterArc = 5; // Диаметр окружности для семейства. Выбрано 5 чтобы его было видно на виде.
    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _revitRepository;
    private readonly FamilyLoadOptions _familyLoadOptions;
    private readonly string _familyTemplatePath;
    private readonly string _familyPath;
    private readonly string _tempDirectory = Path.GetTempPath();
    private readonly string _albumName;

    public TempFamilyDocument(
        ILocalizationService localizationService,
        RevitRepository revitRepository,
        FamilyLoadOptions familyLoadOptions,
        string albumName) {

        _localizationService = localizationService;
        _revitRepository = revitRepository;
        _familyLoadOptions = familyLoadOptions;
        _albumName = albumName;

        string familyTemplatePath = _revitRepository.Application.FamilyTemplatePath;

        _familyTemplatePath = _localizationService.GetLocalizedString(
            "TempFamilyDocument.TemplateFamilyName", familyTemplatePath);
        _familyPath = _localizationService.GetLocalizedString(
            "TempFamilyDocument.FamilyName", _tempDirectory, albumName, _extension);

        CreateDocument();

        FamilySymbol = LoadFamilySymbol();
    }

    public FamilySymbol FamilySymbol { get; }

    public IList<FamilyInstance> PlaceFamilyInstances(
        View view, string number, string revisionNumber, IList<ViewSchedule> viewSchedules) {
        IList<FamilyInstance> familyInstanceList = [];
        foreach(var schedule in viewSchedules) {
            var table_data = schedule.GetTableData();
            var head_data = table_data.GetSectionData(SectionType.Header);
            string result = head_data == null
                ? schedule.Name
                : head_data.GetCellText(0, 0);
            var familyInstance = CreateInstance(view, result, number, revisionNumber);
            familyInstanceList.Add(familyInstance);
        }
        return familyInstanceList;
    }

    public FamilyInstance CreateInstance(View view, string name, string number, string revisionNumber) {
        var xyz = XYZ.Zero;
        var familyInstance = _revitRepository.Document.Create.NewFamilyInstance(xyz, FamilySymbol, view);
        //familyInstance.SetParamValue(ParamFactory.FamilyParamNumber, number);
        //familyInstance.SetParamValue(ParamFactory.FamilyParamName, name);
        //familyInstance.SetParamValue(ParamFactory.FamilyParamRevision, revisionNumber);
        return familyInstance;
    }

    private Document CreateDocument() {
        var document = _revitRepository.Application.NewFamilyDocument(_familyTemplatePath);
        string transactionName = _localizationService.GetLocalizedString("TempFamilyDocument.TransactionName");
        using(var t = document.StartTransaction(transactionName)) {
            CreateCircle(document);
            t.Commit();
        }

        var opt = new SaveAsOptions {
            OverwriteExistingFile = true
        };
        document.SaveAs(_familyPath, opt);
        document.Close(false);
        return document;
    }

    private void CreateCircle(Document document) {
        var viewPlan = new FilteredElementCollector(document)
            .OfClass(typeof(ViewPlan))
            .Cast<ViewPlan>()
            .First();
        var familyCreator = document.FamilyCreate;
        double diameterArc = UnitUtils.ConvertToInternalUnits(_diameterArc, UnitTypeId.Millimeters);
        var plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
        var circle = Arc.Create(plane, diameterArc / 2, 0, 2 * Math.PI);
        familyCreator.NewDetailCurve(viewPlan, circle);
    }

    private FamilySymbol LoadFamilySymbol() {
        FamilySymbol familySymbol = null;

        _revitRepository.Document.LoadFamily(_familyPath, _familyLoadOptions, out var family);
        familySymbol = _revitRepository.GetFamilySymbol(family);

        if(familySymbol != null) {
            if(!familySymbol.IsActive) {
                familySymbol.Activate();
            }
        }
        DeleteFamilySymbol();
        return familySymbol;
    }

    private void DeleteFamilySymbol() {
        try {
            if(File.Exists(_familyPath)) {
                File.Delete(_familyPath);
            }
        } catch(UnauthorizedAccessException) {
        }
    }
}
