using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.SimpleServices;


namespace RevitRoomExtrusion.Models {
    internal class FamilyCreator {
        private readonly ILocalizationService _localizationService;
        private readonly RevitRepository _revitRepository;
        private readonly FamilyLoader _familyLoader;

        public FamilyCreator(
            ILocalizationService localizationService, RevitRepository revitRepository, FamilyLoader familyLoader) {
            _localizationService = localizationService;
            _revitRepository = revitRepository;
            _familyLoader = familyLoader;
        }

        public void CreateFamilies(List<Room> listRoom, View3D view3D, string familyName, double extrusionHeight) {
            List<RoomElement> roomElements = listRoom
                .Select(room => new RoomElement(_revitRepository, room, view3D))
                .ToList();

            IEnumerable<IGrouping<int, RoomElement>> groupedRooms = roomElements
                .GroupBy(re => re.LocationSlab);

            foreach(IGrouping<int, RoomElement> groupRooms in groupedRooms) {
                int locationKey = groupRooms.Key;
                FamilyDocument familyDocument = new FamilyDocument(
                    _localizationService, _revitRepository.Application, locationKey, familyName);

                familyDocument.CreateDocument(groupRooms.ToList(), extrusionHeight);

                FamilySymbol famSymbol = _familyLoader.LoadFamilyInstance(familyDocument.FamilyDocumentPath);

                if(!_familyLoader.IsFamilyInstancePlaced(familyDocument.FamilyDocumentName)) {
                    _familyLoader.PlaceFamilyInstance(famSymbol, groupRooms, locationKey);
                }
            }
        }
    }
}
