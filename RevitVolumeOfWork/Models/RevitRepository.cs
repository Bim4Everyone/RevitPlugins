using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitVolumeOfWork.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;



        public IList<RoomElement> GetRooms() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(SpatialElement))
                .Select(x => new RoomElement((Room)x, Document))
                .ToList();
        }

        public IList<RoomElement> GetRoomsOnActiveView() {
            return new FilteredElementCollector(Document, Document.ActiveView.Id)
                .WhereElementIsNotElementType()
                .OfClass(typeof(SpatialElement))
                .Select(x => new RoomElement((Room) x, Document))
                .ToList();
        }

        public IList<RoomElement> GetSelectedRooms() {
            return ActiveUIDocument.GetSelectedElements()
                .OfType<Room>()
                .Select(x => new RoomElement(x, Document))
                .ToList();
        }

        public Dictionary<int, WallElement> GetGroupedRoomsByWalls(List<RoomElement> rooms) {

            Dictionary<int, WallElement> allWalls = new Dictionary<int, WallElement>();

            foreach(var room in rooms) {
                var walls = room.GetBoundaryWalls();

                foreach(var wall in walls) {
                    int wallId = wall.Id.IntegerValue;
                    if(allWalls.ContainsKey(wall.Id.IntegerValue)) {
                        allWalls[wallId].Rooms.Add(room);
                    } 
                    else {
                        var newWall = new WallElement(wall);
                        newWall.Rooms = new List<RoomElement> { room };
                        allWalls.Add(wallId, newWall);
                    }
                }
            }

            return allWalls;
        }
    }
}