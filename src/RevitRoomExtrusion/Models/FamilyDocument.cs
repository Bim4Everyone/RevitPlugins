using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;


namespace RevitRoomExtrusion.Models;
internal class FamilyDocument {
    private readonly ILocalizationService _localizationService;
    private readonly Application _application;
    private readonly Document _familyDocument;

    private readonly double _location;
    private readonly string _familyName;
    private readonly int _normalDirection = 10;

    public FamilyDocument(
        ILocalizationService localizationService, Application application, double location, string familyName) {
        _localizationService = localizationService;
        _application = application;
        _familyDocument = _application.NewFamilyDocument(GetTemplateFamilyPath());
        _location = location;
        _familyName = familyName;

        SetFamilyNameAndPath();
    }

    public string FamilyDocumentName { get; private set; }
    public string FamilyDocumentPath { get; private set; }

    public Document CreateDocument(List<RoomElement> roomList, double amount) {
        var familyCategory = Category.GetCategory(_familyDocument, BuiltInCategory.OST_Roads);
        var materialElementId = GetMaterialElementId();

        string transactionName = _localizationService.GetLocalizedString("FamilyDocument.TransactionName");
        using(var t = _familyDocument.StartTransaction(transactionName)) {

            _familyDocument.OwnerFamily.FamilyCategory = familyCategory;

            var extrusionList = roomList
                .Select(roomElement => CreateExtrusion(roomElement.ArrArray, amount))
                .ToList();

            if(materialElementId.IsNotNull()) {
                foreach(var extrusion in extrusionList) {
                    extrusion.SetParamValue(BuiltInParameter.MATERIAL_ID_PARAM, materialElementId);
                }
            }
            t.Commit();
        }

        var opt = new SaveAsOptions {
            OverwriteExistingFile = true
        };
        _familyDocument.SaveAs(FamilyDocumentPath, opt);
        _familyDocument.Close(false);
        return _familyDocument;
    }

    private ElementId GetMaterialElementId() {
        string materialName = _localizationService.GetLocalizedString("FamilyDocument.MaterialName");
        return new FilteredElementCollector(_familyDocument)
                .OfClass(typeof(Material))
                .WhereElementIsNotElementType()
                .Where(mat => mat.Name.Equals(materialName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault()
                ?.Id;
    }

    private Extrusion CreateExtrusion(CurveArrArray curveArrArray, double amount) {
        var normal = new XYZ(0, 0, _normalDirection);
        var originPlane = new XYZ(0, 0, 0);
        var plane = Plane.CreateByNormalAndOrigin(normal, originPlane);
        var sketchPlane = SketchPlane.Create(_familyDocument, plane);

        var familyCreator = _familyDocument.FamilyCreate;

        double amountFt = UnitUtils.ConvertToInternalUnits(amount, UnitTypeId.Millimeters);
        var extrusion = familyCreator.NewExtrusion(true, curveArrArray, sketchPlane, amountFt);
        return extrusion;
    }

    private void SetFamilyNameAndPath() {
        string extension = ".rfa";
        string tempDirectory = Path.GetTempPath();
        string familyDocumentName = string.Format(
            _localizationService.GetLocalizedString("FamilyDocument.FamilyName"), _familyName, _location);
        string fileName = $"{familyDocumentName}{extension}";
        string familyDocumentPath = Path.Combine(tempDirectory, fileName);

        FamilyDocumentName = familyDocumentName;
        FamilyDocumentPath = familyDocumentPath;
    }

    private string GetTemplateFamilyPath() {
        string familyTemplatePath = _application.FamilyTemplatePath;
        string localizedString = _localizationService.GetLocalizedString("FamilyDocument.TemplateFamilyName");
        return $"{familyTemplatePath}{localizedString}";
    }
}
