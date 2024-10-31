using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using RevitRoomViewer.ViewModels;

namespace RevitRoomViewer.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public List<RoomElement> GetRoomsWithSettings(List<RoomElement> roomsSettings) {

            var rooms = new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .Where(r => r.Area > 0)
                .ToList();

            var roomsWithSettings = new List<RoomElement>();

            foreach(var room in rooms) {
                var roomSetting = roomsSettings
                    .FirstOrDefault(r => r.Id == room.Id);

                var roomElement = new RoomElement() {
                    Id = room.Id,
                    LevelId = room.LevelId,
                    Name = room.Name,
                    Description = roomSetting?.Description ?? string.Empty,
                    NeedMeasuring = roomSetting?.NeedMeasuring ?? false
                };
                roomsWithSettings.Add(roomElement);
            }
            return roomsWithSettings;
        }

        public List<LevelViewModel> GetLevels(List<RoomElement> roomsSettings = null) {

            var rooms = new List<RoomElement>();

            if(roomsSettings != null) {
                rooms = GetRoomsWithSettings(roomsSettings);
            }

            var levels = new FilteredElementCollector(Document)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToList();

            var levelViewModels = new List<LevelViewModel>();

            foreach(var level in levels) {
                var roomsOnLevel = new List<RoomElement>(
                    rooms.Where(room => room.LevelId == level.Id)
                );

                var levelViewModel = new LevelViewModel(level.Name, level, roomsOnLevel);
                levelViewModels.Add(levelViewModel);
            }
            return levelViewModels;
        }
    }
}
