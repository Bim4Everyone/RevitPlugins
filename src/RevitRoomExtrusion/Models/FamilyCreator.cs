using System.Collections.Generic;
using System.Linq;
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

        public void CreateFamilies(string familyName, double extrusionHeight, List<Room> listRoom, View3D view3D) {
            List<RoomElement> roomElements = listRoom
                .Select(room => {                    
                    return new RoomElement(_revitRepository.Document, room, view3D);
                })
                .ToList();

            IEnumerable<IGrouping<double, RoomElement>>  groupedRooms = roomElements
                .GroupBy(re => re.LocationSlab);

            foreach(IGrouping<double, RoomElement> groupRooms in groupedRooms) {
                double locationKey = groupRooms.Key;
                FamilyDocument familyDocument = new FamilyDocument(_revitRepository.Application, locationKey, familyName);                
                
                familyDocument.CreateDocument(extrusionHeight, groupRooms.ToList());                                                
                
                FamilySymbol famSymbol = _familyLoader.LoadRoomFamily(familyDocument.FamPath);

                if(!_familyLoader.IsFamilyInstancePlaced(familyDocument.FamName)) {
                    _familyLoader.PlaceRoomFamily(famSymbol, groupRooms, locationKey);
                }                                
            }              
        }                
    }
}
