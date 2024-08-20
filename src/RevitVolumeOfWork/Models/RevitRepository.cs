using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Revit;

using RevitVolumeOfWork.ViewModels;

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
                .OfCategory(BuiltInCategory.OST_Rooms)
                .Select(x => new RoomElement((Room)x, Document))
                .ToList();
        }

        public ICollection<WallElement> GetGroupedRoomsByWalls(List<RoomElement> rooms) {
            Dictionary<ElementId, WallElement> allWalls = new Dictionary<ElementId, WallElement>();

            foreach(var room in rooms) {
                var walls = room.GetBoundaryWalls();
                foreach(var wall in walls) {
                    ElementId wallId = wall.Id;
                    if(allWalls.TryGetValue(wall.Id, out WallElement wallElement)) {
                        wallElement.Rooms.Add(room);
                    } 
                    else {
                        var newWall = new WallElement(wall) {
                            Rooms = new List<RoomElement> { room }
                        };

                        allWalls.Add(wallId, newWall);
                    }
                }
            }

            return allWalls.Values;
        }

        public void ClearWallsParameters(IEnumerable<Level> levels) {
            List<ElementFilter> levelFilters = levels
                .Select(x => new ElementLevelFilter(x.Id))
                .Cast<ElementFilter>()
                .ToList();

            LogicalOrFilter orFilter = new LogicalOrFilter(levelFilters);

            FilteredElementCollector collector = new FilteredElementCollector(Document)
                .OfClass(typeof(Wall))
                .WherePasses(orFilter);

            using(Transaction t = Document.StartTransaction("Очистить параметры ВОР стен")) {
                foreach(var wall in collector) {
                    wall.SetParamValue(ProjectParamsConfig.Instance.RelatedRoomName, "");
                    wall.SetParamValue(ProjectParamsConfig.Instance.RelatedRoomNumber, "");
                    wall.SetParamValue(ProjectParamsConfig.Instance.RelatedRoomID, "");
                    wall.SetParamValue(ProjectParamsConfig.Instance.RelatedRoomGroup, "");
                }
                t.Commit();
            }
        }

        public void SetAll(List<LevelViewModel> allLevels, bool value) {
            foreach(var level in allLevels) { level.IsSelected = value; }
        }

        public void InvertAll(List<LevelViewModel> allLevels) {
            foreach(var level in allLevels) { level.IsSelected = !level.IsSelected; }
        }
    }
}