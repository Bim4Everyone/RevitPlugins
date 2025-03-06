using System;
using System.IO;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;


namespace RevitListOfSchedules.Models {
    internal class TempFamilyDocument {
        private readonly ILocalizationService _localizationService;
        private readonly Application _application;
        private readonly Document _familyDocument;
        private readonly string _familyName;
        private readonly int _diameterArc = 5;

        public TempFamilyDocument(
            ILocalizationService localizationService, Application application, string familyName) {
            _localizationService = localizationService;
            _application = application;
            _familyDocument = _application.NewFamilyDocument(GetTemplateFamilyPath());
            _familyName = familyName;

            SetFamilyNameAndPath();
        }

        public string FamilyDocumentName { get; private set; }
        public string FamilyDocumentPath { get; private set; }

        public Document CreateDocument() {
            string transactionName = _localizationService.GetLocalizedString("TempFamilyDocument.TransactionName");
            using(Transaction t = _familyDocument.StartTransaction(transactionName)) {
                CreateParameters();
                CreateCircle();
                t.Commit();
            }
            SaveAsOptions opt = new SaveAsOptions {
                OverwriteExistingFile = true
            };
            _familyDocument.SaveAs(FamilyDocumentPath, opt);
            _familyDocument.Close(false);
            return _familyDocument;
        }

        private void CreateParameters() {
            FamilyManager familyManager = _familyDocument.FamilyManager;
            FamilyParameter familyParameter2 = familyManager
                .AddParameter(RevitRepository.FamilyParamNumber, GroupTypeId.Text, SpecTypeId.String.Text, true);
            FamilyParameter familyParameter1 = familyManager
                .AddParameter(RevitRepository.FamilyParamName, GroupTypeId.Text, SpecTypeId.String.Text, true);
            FamilyParameter familyParameter3 = familyManager
                .AddParameter(RevitRepository.FamilyParamRevision, GroupTypeId.Text, SpecTypeId.String.Text, true);
        }

        private void CreateCircle() {
            ViewSheet viewPlan = new FilteredElementCollector(_familyDocument)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .First();
            Autodesk.Revit.Creation.FamilyItemFactory familyCreator = _familyDocument.FamilyCreate;
            double diameterArc = UnitUtils.ConvertToInternalUnits(_diameterArc, UnitTypeId.Millimeters);
            Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
            Arc circle = Arc.Create(plane, diameterArc / 2, 0, 2 * Math.PI);
            familyCreator.NewDetailCurve(viewPlan, circle);
        }

        private void SetFamilyNameAndPath() {
            string extension = ".rfa";
            string tempDirectory = Path.GetTempPath();
            string familyDocumentName = string.Format(
                _localizationService.GetLocalizedString("TempFamilyDocument.FamilyName"), _familyName);
            string fileName = $"{familyDocumentName}{extension}";
            string familyDocumentPath = Path.Combine(tempDirectory, fileName);

            FamilyDocumentName = familyDocumentName;
            FamilyDocumentPath = familyDocumentPath;
        }

        private string GetTemplateFamilyPath() {
            string familyTemplatePath = _application.FamilyTemplatePath;
            string localizedString = _localizationService.GetLocalizedString("TempFamilyDocument.TemplateFamilyName");
            return $"{familyTemplatePath}{localizedString}";
        }
    }
}
