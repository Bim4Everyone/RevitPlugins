using System.Collections.ObjectModel;
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

        public ObservableCollection<RoomElement> GetRooms(ObservableCollection<RoomElement> roomsWithSettings) {

            Room[] rooms = new FilteredElementCollector(Document, Document.ActiveView.Id)
                .WherePasses(new RoomFilter())
                .Cast<Room>()
                .Where(r => r.Area > 0)
                .ToArray();

            var roomsWithSettingsCollection = new ObservableCollection<RoomElement>();

            foreach(var room in rooms) {
                var roomSetting = roomsWithSettings
                    .FirstOrDefault(r => r.Id == room.Id);

                var roomElement = new RoomElement() {
                    Id = room.Id,
                    LevelId = room.LevelId,
                    Name = room.Name,
                    Description = roomSetting?.Description ?? string.Empty,
                    NeedMeasuring = roomSetting?.NeedMeasuring ?? false
                };
                roomsWithSettingsCollection.Add(roomElement);
            }
            return roomsWithSettingsCollection;
        }

        public ObservableCollection<LevelViewModel> GetLevels(ObservableCollection<RoomElement> rooms) {
            var levels = new FilteredElementCollector(Document)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToList();

            var levelViewModels = new ObservableCollection<LevelViewModel>();

            foreach(var level in levels) {
                var roomsOnLevel = new ObservableCollection<RoomElement>(
                    rooms.Where(room => room.LevelId == level.Id)
                );

                var levelViewModel = new LevelViewModel(level.Name, level, roomsOnLevel);
                levelViewModels.Add(levelViewModel);
            }
            return levelViewModels;
        }
    }
}
