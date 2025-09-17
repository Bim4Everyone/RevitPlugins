using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;


namespace RevitRoomExtrusion.Models;
internal class TempFamilyDocument {
    private const string _extension = ".rfa";
    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _revitRepository;
    private readonly FamilyLoadOptions _familyLoadOptions;
    private readonly string _familyTemplatePath;
    private readonly string _familyPath;
    private readonly string _tempDirectory = Path.GetTempPath();
    private readonly string _familyName;
    private readonly double _location;
    private readonly int _normalDirection = 10;

    public TempFamilyDocument(
        ILocalizationService localizationService,
        RevitRepository revitRepository,
        FamilyLoadOptions familyLoadOptions,
        string familyName,
        int location) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
        _familyLoadOptions = familyLoadOptions;
        _familyName = familyName;
        _location = location;

        _familyTemplatePath = _localizationService.GetLocalizedString(
            "FamilyDocument.TemplateFamilyName", _revitRepository.Application.FamilyTemplatePath);
        _familyPath = _localizationService.GetLocalizedString(
            "FamilyDocument.FamilyName", _tempDirectory, _familyName, _location, _extension);
    }

    //Метод загрузки в проект и получения типоразмера семейства
    public FamilySymbol GetFamilySymbol(List<RoomElement> roomList, double amount, bool joinExtrusionChecked) {
        try {
            CreateFile(roomList, amount, joinExtrusionChecked);
            FamilySymbol familySymbol = null;
            bool loadSuccess = _revitRepository.Document.LoadFamily(_familyPath, _familyLoadOptions, out var family);
            familySymbol = _revitRepository.GetFamilySymbol(family);

            if(!loadSuccess || family == null) {
                return null;
            }
            if(familySymbol != null) {
                if(!familySymbol.IsActive) {
                    familySymbol.Activate();
                }
            }
            return familySymbol;

        } finally {
            DeleteFile();
        }
    }

    //Метод создания файла
    private void CreateFile(List<RoomElement> roomList, double amount, bool joinExtrusionChecked) {
        Document document = null;
        try {
            document = _revitRepository.Application.NewFamilyDocument(_familyTemplatePath);
            string transactionName = _localizationService.GetLocalizedString("FamilyDocument.TransactionName");
            using(var t = document.StartTransaction(transactionName)) {
                CreateExtrusions(document, roomList, amount, joinExtrusionChecked);
                t.Commit();
            }
            var opt = new SaveAsOptions {
                OverwriteExistingFile = true
            };
            document.SaveAs(_familyPath, opt);
        } finally {
            document?.Close(false);
        }
    }

    //Метод удаления файла
    private void DeleteFile() {
        try {
            if(File.Exists(_familyPath)) {
                File.Delete(_familyPath);
            }
        } catch(UnauthorizedAccessException) {
        }
    }

    //Метод создания выдавливаний
    private void CreateExtrusions(Document document, List<RoomElement> roomList, double extrusionHeight, bool joinExtrusionChecked) {
        var familyCategory = Category.GetCategory(document, BuiltInCategory.OST_Roads);
        var materialElementId = GetMaterialElementId(document);
        document.OwnerFamily.FamilyCategory = familyCategory;

        if(!joinExtrusionChecked) {
            foreach(var extrusion in GetExtrusionsList(document, roomList, extrusionHeight)) {
                SetMaterial(document, extrusion);
            }
        } else {
            var extrusionsList = GetExtrusionsList(document, roomList, extrusionHeight);
            var newArrArray = _revitRepository.GetUnitedArrArray(extrusionsList);
            SetMaterial(document, CreateExtrusion(document, newArrArray, extrusionHeight));
            foreach(var extrusion in extrusionsList) {
                document.Delete(extrusion.Id);
            }
        }
    }

    //Метод получения списка выдавливаний
    private List<Extrusion> GetExtrusionsList(Document document, List<RoomElement> roomList, double extrusionHeight) {
        return roomList
            .Select(roomElement => CreateExtrusion(document, roomElement.GetBoundaryArrArray(), extrusionHeight))
            .ToList();
    }

    //Метод назначения материала
    private void SetMaterial(Document document, Extrusion extrusion) {
        var materialElementId = GetMaterialElementId(document);
        if(materialElementId.IsNotNull()) {
            extrusion.SetParamValue(BuiltInParameter.MATERIAL_ID_PARAM, materialElementId);
        }
    }

    //Метод получения нужного материала
    private ElementId GetMaterialElementId(Document document) {
        string materialName = _localizationService.GetLocalizedString("FamilyDocument.MaterialName");
        return new FilteredElementCollector(document)
                .OfClass(typeof(Material))
                .WhereElementIsNotElementType()
                .Where(mat => mat.Name.Equals(materialName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault()
                ?.Id;
    }

    //Метод создания одного выдавливания
    private Extrusion CreateExtrusion(Document document, CurveArrArray curveArrArray, double amount) {
        var normal = new XYZ(0, 0, _normalDirection);
        var originPlane = new XYZ(0, 0, 0);
        var plane = Plane.CreateByNormalAndOrigin(normal, originPlane);
        var sketchPlane = SketchPlane.Create(document, plane);
        var familyCreator = document.FamilyCreate;
        double amountFt = UnitUtils.ConvertToInternalUnits(amount, UnitTypeId.Millimeters);
        var extrusion = familyCreator.NewExtrusion(true, curveArrArray, sketchPlane, amountFt);
        return extrusion;
    }
}
