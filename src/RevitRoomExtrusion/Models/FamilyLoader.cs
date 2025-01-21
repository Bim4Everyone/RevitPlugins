using System;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;


namespace RevitRoomExtrusion.Models {
    internal class FamilyLoader {

        private readonly RevitRepository _revitRepository;

        public FamilyLoader(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public FamilySymbol LoadRoomFamily(string path) {
            Family family = null;
            FamilySymbol familySymbol = null;
            FamilyLoadOptions loadOptions = new FamilyLoadOptions();

            using(Transaction t = _revitRepository.Document.StartTransaction("BIM: Загрузка семейства дорожек")) {
                _revitRepository.Document.LoadFamily(path, loadOptions, out family);

                familySymbol = family.GetFamilySymbolIds()
                    .Select(id => _revitRepository.Document.GetElement(id) as FamilySymbol)
                    .FirstOrDefault(symbol => symbol != null);

                if(!familySymbol.IsActive) {
                    familySymbol.Activate();
                    _revitRepository.Document.Regenerate();
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
                TaskDialog.Show("BIM", $"Ошибка удаления файла семейства. Удалите файл {famPath} вручную.");
            }
        }

        public void PlaceRoomFamily(FamilySymbol symbol, IGrouping<double, RoomElement> groupRooms, double location) {
            double locationRoom = groupRooms
                .FirstOrDefault().LocationRoom;
            double locationFt = UnitUtils.ConvertToInternalUnits(location, UnitTypeId.Millimeters);
            double locationPlace = locationFt - locationRoom;

            using(Transaction t = _revitRepository.Document.StartTransaction("BIM: Размещение семейства дорожек")) {
                XYZ xyz = new XYZ(0, 0, locationPlace);
                FamilyInstance familyInstance = _revitRepository.Document.Create.NewFamilyInstance(
                    xyz,
                    symbol,
                    StructuralType.NonStructural);
                t.Commit();
            }
        }

        public bool IsFamilyInstancePlaced(string familyName) {
            return new FilteredElementCollector(_revitRepository.Document)
                .OfCategory(BuiltInCategory.OST_Roads)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Any(familyInstance => familyInstance.Name == familyName);
        }
    }
}
