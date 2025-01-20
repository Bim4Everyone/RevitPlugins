using System;
using System.Collections.Generic;
using System.Linq;
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

        public void CreatingFamilies(string familyName, 
            string extrusionHeight, 
            List<Room> listRoom,
            View3D view3D,
            IProgress<int> progress = null,
            CancellationToken ct = default) {             


            foreach(IGrouping<double, RoomElement> groupRooms in GetGropRoomElements(listRoom, view3D, progress)) {
                ct.ThrowIfCancellationRequested();

                double locationKey = groupRooms.Key;
                double extrusionHeightDouble = Convert.ToDouble(extrusionHeight);

                FamilyDocument familyDocument = new FamilyDocument(_revitRepository.Application, locationKey, familyName);                
                familyDocument.CreateDocument(extrusionHeightDouble, groupRooms.ToList());
                                                
                FamilySymbol famSymbol = null;               
                famSymbol = _familyLoader.LoadRoomFamily(familyDocument.FamPath);

                _familyLoader.PlaceRoomFamily(famSymbol, groupRooms, locationKey);                
            }              
        }

        private IEnumerable<IGrouping<double, RoomElement>> GetGropRoomElements(List<Room> listRoom, 
                                                                                View3D view3D, 
                                                                                IProgress<int> progress) {
            int progressCount = 0;
            List<RoomElement> roomElements = listRoom
                .Select(room => {
                    progress.Report(progressCount++);
                    return new RoomElement(_revitRepository.Document, room, view3D);
                })
                .ToList();
                return roomElements.GroupBy(re => re.LocationSlab);
        }         
    }
}
