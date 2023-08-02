using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

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
                .WhereElementIsNotElementType()
                .OfClass(typeof(SpatialElement))
                .Select(x => new RoomElement((Room)x, Document))
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

        public void CleanWallsParameters(List<Level> levels) {
            List<ElementFilter> levelFilters = levels.Select(x => (ElementFilter) new ElementLevelFilter(x.Id)).ToList();

            LogicalOrFilter orFIlter = new LogicalOrFilter(levelFilters);

            FilteredElementCollector collector = new FilteredElementCollector(Document)
                .OfClass(typeof(Wall))
                .WherePasses(orFIlter);

            using(Transaction t = Document.StartTransaction("Очистить параметры ВОР стен")) {
                foreach(var wall in collector) {
                    wall.SetProjectParamValue(ProjectParamsConfig.Instance.RelatedRoomName.Name, "");
                    wall.SetProjectParamValue(ProjectParamsConfig.Instance.RelatedRoomNumber.Name, "");
                    wall.SetProjectParamValue(ProjectParamsConfig.Instance.RelatedRoomID.Name, "");
                    wall.SetProjectParamValue(ProjectParamsConfig.Instance.RelatedApartmentNumber.Name, "");
                }
                t.Commit();
            }
        }

        public void SetAll(ObservableCollection<LevelViewModel> allLevels, bool value) {
            foreach(var level in allLevels) { level.IsSelected = value; }
        }

        public void InvertAll(ObservableCollection<LevelViewModel> allLevels) {
            foreach(var level in allLevels) { level.IsSelected = !level.IsSelected; }
        }
    }
}