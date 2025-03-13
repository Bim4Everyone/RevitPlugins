using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitListOfSchedules.Interfaces;
using RevitListOfSchedules.ViewModels;


namespace RevitListOfSchedules.Models {
    internal class TempFamilyDocument : IFamilyDocument {
        private const int _diameterArc = 5;
        private readonly ILocalizationService _localizationService;
        private readonly RevitRepository _revitRepository;
        private readonly FamilyLoadOptions _familyLoadOptions;
        private readonly string _albumName;
        private readonly string _familyTemplatePath;
        private readonly string _familyName;
        private readonly string _familyPath;
        private readonly FamilySymbol _familySymbol;

        public TempFamilyDocument(
            ILocalizationService localizationService,
            RevitRepository revitRepository,
            FamilyLoadOptions familyLoadOptions,
            string albumName) {
            _localizationService = localizationService;
            _revitRepository = revitRepository;
            _familyLoadOptions = familyLoadOptions;
            _albumName = albumName;
            _familyTemplatePath = GetTemplateFamilyPath();
            _familyName = GetFamilyName();
            _familyPath = GetFamilyPath();
            CreateDocument();
            _familySymbol = LoadFamilySymbol();
        }

        public FamilySymbol FamilySymbol => _familySymbol;

        public IList<FamilyInstance> PlaceFamilyInstances(
            View view, SheetViewModel sheetViewModel, IList<ViewSchedule> viewSchedules) {
            IList<FamilyInstance> familyInstanceList = [];
            foreach(ViewSchedule schedule in viewSchedules) {
                TableData table_data = schedule.GetTableData();
                TableSectionData head_data = table_data.GetSectionData(SectionType.Header);
                string result = head_data == null
                    ? schedule.Name
                    : head_data.GetCellText(0, 0);
                FamilyInstance familyInstance = CreateInstance(
                    view, result, sheetViewModel.Number, sheetViewModel.RevisionNumber);
                familyInstanceList.Add(familyInstance);
            }
            return familyInstanceList;
        }

        public FamilyInstance CreateInstance(View view, string name, string number, string revisionNumber) {
            XYZ xyz = XYZ.Zero;
            FamilyInstance familyInstance = _revitRepository.Document.Create.NewFamilyInstance(xyz, _familySymbol, view);
            familyInstance.SetParamValue(ParamFactory.FamilyParamNumber, number);
            familyInstance.SetParamValue(ParamFactory.FamilyParamName, name);
            familyInstance.SetParamValue(ParamFactory.FamilyParamRevision, revisionNumber);
            return familyInstance;
        }

        private string GetTemplateFamilyPath() {
            string familyTemplatePath = _revitRepository.Application.FamilyTemplatePath;
            string localizedString = _localizationService.GetLocalizedString("TempFamilyDocument.TemplateFamilyName");
            return $"{familyTemplatePath}{localizedString}";
        }

        private string GetFamilyName() {
            return string.Format(
                _localizationService.GetLocalizedString("TempFamilyDocument.FamilyName"), _albumName);
        }

        private string GetFamilyPath() {
            string extension = ".rfa";
            string tempDirectory = Path.GetTempPath();
            string familyDocumentName = string.Format(_familyName, _albumName);
            string fileName = $"{familyDocumentName}{extension}";
            return Path.Combine(tempDirectory, fileName);
        }

        private Document CreateDocument() {
            Document document = _revitRepository.Application.NewFamilyDocument(_familyTemplatePath);
            string transactionName = _localizationService.GetLocalizedString("TempFamilyDocument.TransactionName");
            using(Transaction t = document.StartTransaction(transactionName)) {
                CreateParameters(document);
                CreateCircle(document);
                t.Commit();
            }
            SaveAsOptions opt = new SaveAsOptions {
                OverwriteExistingFile = true
            };
            document.SaveAs(_familyPath, opt);
            document.Close(false);
            return document;
        }

        private void CreateParameters(Document document) {
            FamilyManager familyManager = document.FamilyManager;
            familyManager.AddParameter(ParamFactory.FamilyParamNumber, GroupTypeId.Text, SpecTypeId.String.Text, true);
            familyManager.AddParameter(ParamFactory.FamilyParamName, GroupTypeId.Text, SpecTypeId.String.Text, true);
            familyManager.AddParameter(ParamFactory.FamilyParamRevision, GroupTypeId.Text, SpecTypeId.String.Text, true);
        }

        private void CreateCircle(Document document) {
            ViewSheet viewPlan = new FilteredElementCollector(document)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .First();
            Autodesk.Revit.Creation.FamilyItemFactory familyCreator = document.FamilyCreate;
            double diameterArc = UnitUtils.ConvertToInternalUnits(_diameterArc, UnitTypeId.Millimeters);
            Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
            Arc circle = Arc.Create(plane, diameterArc / 2, 0, 2 * Math.PI);
            familyCreator.NewDetailCurve(viewPlan, circle);
        }

        private FamilySymbol LoadFamilySymbol() {
            FamilySymbol familySymbol = null;
            string transactionNameLoad = _localizationService.GetLocalizedString("TempFamilyDocument.TransactionNameLoad");
            using(Transaction t = _revitRepository.Document.StartTransaction(transactionNameLoad)) {

                _revitRepository.Document.LoadFamily(_familyPath, _familyLoadOptions, out Family family);

                familySymbol = _revitRepository.GetFamilySymbol(family);

                if(familySymbol != null) {
                    if(!familySymbol.IsActive) {
                        familySymbol.Activate();
                    }
                }
                t.Commit();
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
}
