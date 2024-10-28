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

        public List<LevelViewModel> GetLevelsWithRooms(Dictionary<string, RoomElement> roomsWithSettings) {

            var levels = new FilteredElementCollector(Document)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToList();

            var rooms = new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .ToList();

            var levelViewModels = new List<LevelViewModel>();

            foreach(var level in levels) {

                var roomsOnLevel = rooms
                    .Where(room => room.LevelId == level.Id)
                    .Select(room => {
                        roomsWithSettings.TryGetValue(room.Id.ToString(), out RoomElement roomSetting);
                        var roomElement = new RoomElement(room) {
                            Description = roomSetting?.Description ?? string.Empty,
                            NeedMeasuring = roomSetting?.NeedMeasuring ?? false
                        };
                        return roomElement;
                    })
                    .ToList();


                var levelViewModel = new LevelViewModel(level.Name, level, roomsOnLevel);
                levelViewModels.Add(levelViewModel);
            }

            return levelViewModels;
        }

    }
}
