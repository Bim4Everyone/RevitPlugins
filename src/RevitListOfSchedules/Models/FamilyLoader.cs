using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitListOfSchedules.ViewModels;

namespace RevitListOfSchedules.Models {
    internal class FamilyLoader {
        private readonly ILocalizationService _localizationService;
        private readonly RevitRepository _revitRepository;
        private readonly FamilyLoadOptions _familyLoadOptions;

        public FamilyLoader(
            ILocalizationService localizationService,
            RevitRepository revitRepository,
            FamilyLoadOptions familyLoadOptions) {
            _revitRepository = revitRepository;
            _familyLoadOptions = familyLoadOptions;
            _localizationService = localizationService;
        }

        public FamilySymbol LoadFamilyInstance(string path) {
            Family family = null;
            FamilySymbol familySymbol = null;

            string transactionNameLoad = _localizationService.GetLocalizedString("FamilyLoader.TransactionNameLoad");
            using(Transaction t = _revitRepository.Document.StartTransaction(transactionNameLoad)) {
                _revitRepository.Document.LoadFamily(path, _familyLoadOptions, out family);

                familySymbol = _revitRepository.GetFamilySymbol(family);

                if(familySymbol != null) {
                    if(!familySymbol.IsActive) {
                        familySymbol.Activate();
                    }
                }
                t.Commit();
            }
            DeleteRoomFamily(path);
            return familySymbol;
        }

        private void DeleteRoomFamily(string famPath) {
            try {
                if(File.Exists(famPath)) {
                    File.Delete(famPath);
                }
            } catch(UnauthorizedAccessException) {
            }
        }

        public IList<FamilyInstance> PlaceFamilyInstance(
            FamilySymbol symbol, View view, SheetViewModel sheetViewModel, IList<ViewSchedule> viewSchedules) {



            IList<FamilyInstance> familyInstanceList = [];

            string transactionNamePlace = _localizationService.GetLocalizedString("FamilyLoader.TransactionNamePlace");
            using(Transaction t = _revitRepository.Document.StartTransaction(transactionNamePlace)) {
                foreach(ViewSchedule schedule in viewSchedules) {
                    TableData table_data = schedule.GetTableData();
                    TableSectionData head_data = table_data.GetSectionData(SectionType.Header);
                    string result = head_data == null
                        ? schedule.Name
                        : head_data.GetCellText(0, 0);
                    XYZ xyz = XYZ.Zero;
                    FamilyInstance familyInstance = _revitRepository.Document.Create.NewFamilyInstance(xyz, symbol, view);
                    familyInstanceList.Add(familyInstance);
                    familyInstance.SetParamValue(RevitRepository.FamilyParamNumber, sheetViewModel.Number);
                    familyInstance.SetParamValue(RevitRepository.FamilyParamName, result);
                    familyInstance.SetParamValue(RevitRepository.FamilyParamRevision, sheetViewModel.RevisionNumber);
                }
                t.Commit();
            }
            return familyInstanceList;
        }

        public bool IsFamilyInstancePlaced(string familyName) {
            var family = new FilteredElementCollector(_revitRepository.Document)
                .OfClass(typeof(Family))
                .FirstOrDefault(f => f.Name.Equals(familyName, StringComparison.InvariantCultureIgnoreCase));
            if(family != null) {
                var symbolId = new FilteredElementCollector(_revitRepository.Document)
                    .WherePasses(new FamilySymbolFilter(family.Id))
                    .Select(selector => selector.Id)
                    .FirstOrDefault();
                if(symbolId.IsNotNull()) {
                    return new FilteredElementCollector(_revitRepository.Document)
                        .WherePasses(new FamilyInstanceFilter(_revitRepository.Document, symbolId))
                        .Any();
                }
            }
            return false;
        }
    }
}
