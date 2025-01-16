using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
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

        public void CreatingFamilyes(string familyName, 
            string extrusionHeight, 
            List<Room> listRoom,
            IProgress<int> progress = null,
            CancellationToken ct = default) {             

            View3D view3D = null;
            using(Transaction tra = new Transaction(_revitRepository.Document, "BIM: Создание 3D вида проверки дорожек")) {
                tra.Start();
                string userName = _revitRepository.Application.Username;
                string name3Dview = $"${userName}/{familyName}/Проверка дорожек";
                view3D = _revitRepository.GetView3D(name3Dview);
                tra.Commit();
            }

            List<RoomElement> roomElements = new List<RoomElement>();
            int progressCount = 0;
            foreach(Room room in listRoom) {
                progress.Report(progressCount++);
                RoomElement roomElement = new RoomElement(_revitRepository.Document, room, view3D);
                roomElements.Add(roomElement);
            }

            var groupedRooms = roomElements.GroupBy(re => re.LocationSlab);
            
            foreach(IGrouping<double, RoomElement> groupRooms in groupedRooms) {
                ct.ThrowIfCancellationRequested();
                
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
                using(Transaction tr = new Transaction(_revitRepository.Document, "BIM: Загрузка семейства дорожек")) {
                    tr.Start();                    
                    famSymbol = _familyLoader.LoadRoomFamily(famPath);
                    tr.Commit();
                }

                File.Delete(famPath);

                double locationRoom = groupRooms.FirstOrDefault().LocationRoom;                
                double locationFt = UnitUtils.ConvertToInternalUnits(location, UnitTypeId.Millimeters);
                double locationPlace = locationFt - locationRoom;

                bool isFamilySymbolPlaced = _familyLoader.IsFamilyInstancePlaced(familyDocument.FamName);

                using(Transaction ta = new Transaction(_revitRepository.Document, "BIM: Вставка семейства дорожек")) {
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
