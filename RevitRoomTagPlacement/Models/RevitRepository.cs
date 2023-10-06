using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows.Controls;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Revit;

using pyRevitLabs.Json.Linq;

using RevitRoomTagPlacement.ViewModels;

namespace RevitRoomTagPlacement.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public List<Room> GetSelectedRooms() {
            return ActiveUIDocument.GetSelectedElements()
                .Where(x => x is Room)
                .OfType<Room>()
                .ToList();
        }

        public List<Room> GetRoomsOnActiveView() {
            return new FilteredElementCollector(Document, Document.ActiveView.Id)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Rooms)
                .OfType<Room>()
                .ToList();
        }

        public List<RoomTagTypeModel> GetRoomTags() {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(FamilySymbol))
                .OfType<RoomTagType>()
                .Select(x => new RoomTagTypeModel(x))
                .ToList();
        }

        public ObservableCollection<string> GetRoomNames(IList<RoomGroupViewModel> RoomGroups) {
            var selectedGroups = RoomGroups.Where(x => x.IsChecked);
            IEnumerable<string> uniqueNames = new List<string>();

            if(selectedGroups.Count() > 0) {
               uniqueNames = new List<string>(selectedGroups.First().GroupRoomNames);

                foreach(var group in selectedGroups) {
                    uniqueNames = uniqueNames.Intersect(group.GroupRoomNames);
                }
            }

            return new ObservableCollection<string>(uniqueNames);
        }

        public void PlaceTagsCommand(IList<RoomGroupViewModel> RoomGroups, 
                                    ElementId SelectedTagType, 
                                    GroupPlacementWay groupPlacementWay,
                                    PositionPlacementWay positionPlacementWay,
                                    string roomName = "") {
            var selectedGroups = RoomGroups.Where(x => x.IsChecked);

            if(groupPlacementWay == GroupPlacementWay.EveryRoom) {
                var rooms = selectedGroups.SelectMany(x => x.Rooms);
                PlaceTags(rooms, SelectedTagType, positionPlacementWay);

            } 
            else if(groupPlacementWay == GroupPlacementWay.OneRoomPerGroupRandom) {
                var rooms = selectedGroups.Select(x => x.Rooms.OrderByDescending(r => r.Area).First());
                PlaceTags(rooms, SelectedTagType, positionPlacementWay);
            } 
            else if(groupPlacementWay == GroupPlacementWay.OneRoomPerGroupByName) {
                var rooms = selectedGroups.Select(x => x.Rooms.Where(y => y.GetParamValue<string>(BuiltInParameter.ROOM_NAME) == roomName).First());
                PlaceTags(rooms, SelectedTagType, positionPlacementWay);
            }
        }

        public void PlaceTags(IEnumerable<Room> Rooms, 
                              ElementId SelectedTagType,
                              PositionPlacementWay positionPlacementWay) {           
            using(Transaction t = Document.StartTransaction("Маркировать помещения")) {
                foreach(var room in Rooms) {

                    ElementOwnerViewFilter viewFilter = new ElementOwnerViewFilter(Document.ActiveView.Id);

                    var depElements = room
                        .GetDependentElements(viewFilter)
                        .Select(x => Document.GetElement(x))
                        .Select(x => x.GetTypeId())
                        .ToList();

                    if(!depElements.Contains(SelectedTagType)) {
                        RoomPathFinder pathFinder = new RoomPathFinder(room);
                        UV point = pathFinder.GetPointByPlacementWay(positionPlacementWay, Document.ActiveView);
                        LocationPoint roomLocation = (LocationPoint) room.Location;
                        XYZ testPoint = new XYZ(point.U, point.V, roomLocation.Point.Z);

                        if(!room.IsPointInRoom(testPoint)) point = pathFinder.GetPointByPath();

                        var newTag = Document.Create.NewRoomTag(
                            new LinkElementId(room.Id), 
                            point, 
                            Document.ActiveView.Id);

                        newTag.ChangeTypeId(SelectedTagType);
                    }
                }
                t.Commit();
            }
        }
    }
}
