using System;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitListOfSchedules.Models;
internal class TempFamilyDocument {
    private const string _extension = ".rfa";
    private const int _diameterArc = 5; // Диаметр окружности для семейства. Выбрано 5, чтобы его было видно на виде.
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

    public FamilySymbol GetFamilySymbol(string familyTemplatePath, string albumName)
    {
        string familyPath = _localizationService.GetLocalizedString(
            "TempFamilyDocument.FamilyName", _tempDirectory, albumName, _extension);
        try {
            CreateFile(familyTemplatePath, familyPath);
            
            bool success = LoadFamily(familyPath, out var family);

            if (!success) {
                string famName = Path.GetFileNameWithoutExtension(familyPath);

                var existingFamily = new FilteredElementCollector(_revitRepository.Document)
                    .OfClass(typeof(Family))
                    .Cast<Family>()
                    .FirstOrDefault(f => f.Name.Equals(famName, StringComparison.OrdinalIgnoreCase));

                if (existingFamily != null) {
                    _revitRepository.Document.Delete(existingFamily.Id);
                    success = LoadFamily(familyPath, out family);
                }
            }

            if (!success)
                throw new InvalidOperationException();

            var familySymbol = _revitRepository.GetFamilySymbol(family);

            if (familySymbol == null)
                throw new InvalidOperationException();

            if (!familySymbol.IsActive)
                familySymbol.Activate();

            return familySymbol;
        }
        catch (Exception ex) {
            throw new InvalidOperationException(
                _localizationService.GetLocalizedString("TempFamilyDocument.ExceptionFamily", albumName), ex);
        }
        finally {
            try {
                DeleteFile(familyPath);
            } catch {
                // Не важное
            }
        }
    }
    
    private bool LoadFamily(string familyPath, out Family family) {
        return _revitRepository.Document.LoadFamily(familyPath, _familyLoadOptions, out family);
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
        } catch {
            // Не важное
        }
    }
}
