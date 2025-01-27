using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;


namespace RevitRoomExtrusion.Models {
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
            Category familyCategory = Category.GetCategory(_familyDocument, BuiltInCategory.OST_Roads);
            ElementId materialElementId = GetMaterialElementId();

            string transactionName = _localizationService.GetLocalizedString("FamilyDocument.TransactionName");
            using(Transaction t = _familyDocument.StartTransaction(transactionName)) {

                _familyDocument.OwnerFamily.FamilyCategory = familyCategory;

                List<Extrusion> extrusionList = roomList
                    .Select(roomElement => CreateExtrusion(roomElement.ArrArray, amount))
                    .ToList();

                if(materialElementId != null) {
                    foreach(Extrusion extrusion in extrusionList) {
                        Parameter extrusionParameter = extrusion.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
                        extrusionParameter.Set(materialElementId);
                    }
                }
                t.Commit();
            }

            SaveAsOptions opt = new SaveAsOptions {
                OverwriteExistingFile = true
            };
            _familyDocument.SaveAs(FamilyDocumentPath, opt);
            _familyDocument.Close(false);
            return _familyDocument;
        }

        private ElementId GetMaterialElementId() {
            string materialName = _localizationService.GetLocalizedString("FamilyDocument.MaterialName");
            var materials = new FilteredElementCollector(_familyDocument)
                    .OfClass(typeof(Material))
                    .WhereElementIsNotElementType()
                    .ToElements();
            return materials
                .FirstOrDefault(mat => mat.Name.Equals(materialName, StringComparison.OrdinalIgnoreCase))
                ?.Id;
        }

        private Extrusion CreateExtrusion(CurveArrArray curveArrArray, double amount) {
            XYZ normal = new XYZ(0, 0, _normalDirection);
            XYZ originPlane = new XYZ(0, 0, 0);
            Plane plane = Plane.CreateByNormalAndOrigin(normal, originPlane);
            SketchPlane sketchPlane = SketchPlane.Create(_familyDocument, plane);

            Autodesk.Revit.Creation.FamilyItemFactory familyCreator = _familyDocument.FamilyCreate;

            double amountFt = UnitUtils.ConvertToInternalUnits(amount, UnitTypeId.Millimeters);
            Extrusion extrusion = familyCreator.NewExtrusion(true, curveArrArray, sketchPlane, amountFt);
            return extrusion;
        }

        private void SetFamilyNameAndPath() {
            string extension = ".rfa";
            string tempDirectory = Path.GetTempPath();
            string familyDocumentName = String.Format(
                _localizationService.GetLocalizedString("FamilyDocument.FamilyName"), _familyName, _location);
            string fileName = String.Format("{0}{1}", familyDocumentName, extension);
            string familyDocumentPath = Path.Combine(tempDirectory, fileName);

            FamilyDocumentName = familyDocumentName;
            FamilyDocumentPath = familyDocumentPath;
        }

        private string GetTemplateFamilyPath() {
            string familyTemplatePath = _application.FamilyTemplatePath;
            string localizedString = _localizationService.GetLocalizedString("FamilyDocument.TemplateFamilyName");
            return String.Format("{0}{1}", familyTemplatePath, localizedString);
        }
    }
}
