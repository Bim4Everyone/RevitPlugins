using System;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitListOfSchedules.Models;
internal class TempFamilyDocument {
    private const string _extension = ".rfa";
    private const int _diameterArc = 5; // Диаметр окружности для семейства. Выбрано 5 чтобы его было видно на виде.
    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _revitRepository;
    private readonly FamilyLoadOptions _familyLoadOptions;
    private readonly string _tempDirectory = Path.GetTempPath();

    public TempFamilyDocument(
        ILocalizationService localizationService,
        RevitRepository revitRepository,
        FamilyLoadOptions familyLoadOptions) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
        _familyLoadOptions = familyLoadOptions;
    }

    public FamilySymbol GetFamilySymbol(string familyTemplatePath, string albumName) {
        string familyPath = _localizationService.GetLocalizedString(
            "TempFamilyDocument.FamilyName", _tempDirectory, albumName, _extension);
        try {
            CreateFile(familyTemplatePath, familyPath);
            FamilySymbol familySymbol = null;
            bool loadSuccess = _revitRepository.Document.LoadFamily(familyPath, _familyLoadOptions, out var family);
            familySymbol = _revitRepository.GetFamilySymbol(family);

            if(!loadSuccess || family == null) {
                DeleteFile(familyPath);
                return null;
            }
            if(familySymbol != null) {
                if(!familySymbol.IsActive) {
                    familySymbol.Activate();
                }
            }
            DeleteFile(familyPath);
            return familySymbol;

        } finally {
            DeleteFile(familyPath);
        }
    }

    private void CreateFile(string familyTemplatePath, string familyPath) {
        Document document = null;
        try {
            document = _revitRepository.Application.NewFamilyDocument(familyTemplatePath);
            string transactionName = _localizationService.GetLocalizedString("TempFamilyDocument.TransactionName");
            using(var t = document.StartTransaction(transactionName)) {
                CreateCircle(document);
                t.Commit();
            }
            var opt = new SaveAsOptions {
                OverwriteExistingFile = true
            };
            document.SaveAs(familyPath, opt);

        } finally {
            document?.Close(false);
        }
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

    private void DeleteFile(string familyPath) {
        try {
            if(File.Exists(familyPath)) {
                File.Delete(familyPath);
            }
        } catch(UnauthorizedAccessException) {
        }
    }
}
