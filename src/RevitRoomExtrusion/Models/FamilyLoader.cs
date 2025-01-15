using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using System.Linq;

namespace RevitRoomExtrusion.Models {
    internal class FamilyLoader {

        private readonly RevitRepository _revitRepository;         
        public FamilyLoader(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }
        public FamilySymbol LoadRoomFamily(string path) {
            Family family = null;
            FamilyLoadOptions loadOptions = new FamilyLoadOptions();
            _revitRepository.Document.LoadFamily(path, loadOptions, out family);

            FamilySymbol familySymbol = null;
            foreach(ElementId id in family.GetFamilySymbolIds()) {
                Element element = _revitRepository.Document.GetElement(id);
                familySymbol = element as FamilySymbol;
                break;
            }
            if(!familySymbol.IsActive) {
                familySymbol.Activate();
                _revitRepository.Document.Regenerate();
            }
            return familySymbol;
        }
        private class FamilyLoadOptions : IFamilyLoadOptions {
            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues) {
                overwriteParameterValues = true;
                return true;
            }
            public bool OnSharedFamilyFound(
                Family sharedFamily,
                bool familyInUse,
                out FamilySource source,
                out bool overwriteParameterValues) {
                source = FamilySource.Project;
                overwriteParameterValues = true;
                return true;
            }
        }
        public void PlaceFamily(FamilySymbol symbol, double zLocation) {
            XYZ xyz = new XYZ(0, 0, zLocation);
            FamilyInstance familyInstance = _revitRepository.Document.Create.NewFamilyInstance(
                xyz, 
                symbol, 
                StructuralType.NonStructural);
        }
        public bool IsFamilyInstancePlaced(string familyName) {
            bool resultFam = false;
            var familyInstances = new FilteredElementCollector(_revitRepository.Document)
                .OfCategory(BuiltInCategory.OST_Roads)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<FamilyInstance>();

            foreach(var familyInstance in familyInstances) {
                if(familyInstance.Symbol.Name == familyName) {
                    resultFam = true;
                    break;
                }
            }
            return resultFam;
        }
    }
}
