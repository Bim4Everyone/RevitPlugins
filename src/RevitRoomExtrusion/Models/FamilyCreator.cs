using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;


namespace RevitRoomExtrusion.Models {     
    internal class FamilyCreator {

        private readonly RevitRepository _revitRepository;
        private readonly FamilyLoader _familyLoader;

        public FamilyCreator(RevitRepository revitRepository) {            
            _revitRepository = revitRepository;
            _familyLoader = new FamilyLoader(revitRepository);
        }

        public void CreatingFamilyes(string familyName, string extrusionHeight, List<Room> listRoom) {

            View3D view3D = null;
            using(Transaction tra = new Transaction(_revitRepository.Document, "Создание 3D вида")) {
                tra.Start();
                view3D = _revitRepository.GetView3D(familyName);
                tra.Commit();
            }

            List<RoomElement> roomElements = new List<RoomElement>();
            foreach(Room room in listRoom) {
                RoomElement roomElement = new RoomElement(_revitRepository.Document, room, view3D);
                roomElements.Add(roomElement);
            }

            var groupedRooms = roomElements.GroupBy(re => re.LocationSlab);

            foreach(IGrouping<double, RoomElement> groupRooms in groupedRooms) {
                double location = groupRooms.Key;
                double extrusionHeightDouble = Convert.ToDouble(extrusionHeight);

                FamilyDocument familyDocument = new FamilyDocument(
                    _revitRepository.Application,
                    extrusionHeightDouble,
                    location,
                    groupRooms.ToList(),
                    familyName);

                familyDocument.CreateFamily();

                string famPath = familyDocument.FamPath;
                FamilySymbol famSymbol = null;
                using(Transaction tr = new Transaction(_revitRepository.Document, "Загрузка семейства")) {
                    tr.Start();                    
                    famSymbol = _familyLoader.LoadRoomFamily(famPath);
                    tr.Commit();
                }

                File.Delete(famPath);

                double locationRoom = groupRooms.FirstOrDefault().LocationRoom;
                double locationPlace = location - locationRoom;
                bool isFamilySymbolPlaced = _familyLoader.IsFamilyInstancePlaced(familyDocument.FamName);

                using(Transaction ta = new Transaction(_revitRepository.Document, "Вставка семейства")) {
                    ta.Start();
                    if(!isFamilySymbolPlaced) {
                        _familyLoader.PlaceFamily(famSymbol, locationPlace);
                    }                    
                    ta.Commit();
                }
            }
        }
    }
}
