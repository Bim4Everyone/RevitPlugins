using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Security;
using System.Windows.Controls;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using pyRevitLabs.Json.Linq;

using RevitRoomTagPlacement.ViewModels;

using View = Autodesk.Revit.DB.View;

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

        public void PlaceTagsByPositionAndGroup(IList<RoomGroupViewModel> RoomGroups, 
                                            ElementId SelectedTagType, 
                                            GroupPlacementWay groupPlacementWay,
                                            PositionPlacementWay positionPlacementWay,
                                            string roomName = "") {
            var selectedGroups = RoomGroups.Where(x => x.IsChecked);
            List<Room> rooms = new List<Room>();

            if(groupPlacementWay == GroupPlacementWay.EveryRoom) {
                rooms = selectedGroups.SelectMany(x => x.Rooms).ToList();

            } 
            else if(groupPlacementWay == GroupPlacementWay.OneRoomPerGroupRandom) {
                RevitParam sectionParam = SharedParamsConfig.Instance.RoomSectionShortName;
                rooms = selectedGroups
                    .SelectMany(x => x.Rooms
                        .GroupBy(r => r.GetParamValue<string>(sectionParam))
                        .Select(g => g.OrderByDescending(r => r.Area)
                        .First()))
                    .ToList();

            } 
            else if(groupPlacementWay == GroupPlacementWay.OneRoomPerGroupByName) {
                RevitParam sectionParam = SharedParamsConfig.Instance.RoomSectionShortName;
                rooms = selectedGroups
                    .SelectMany(x => x.Rooms
                        .GroupBy(r => r.GetParamValue<string>(sectionParam))
                        .Select(g => g.Where(y => y.GetParamValue<string>(BuiltInParameter.ROOM_NAME) == roomName)
                        .First()))
                    .ToList();
            }

            View activeView = Document.ActiveView;
            ElementOwnerViewFilter viewFilter = new ElementOwnerViewFilter(activeView.Id);

            using(Transaction t = Document.StartTransaction("Маркировать помещения")) {
                foreach(var room in rooms) {
                    var depElements = room
                        .GetDependentElements(viewFilter)
                        .Select(x => Document.GetElement(x))
                        .Select(x => x.GetTypeId())
                        .ToList();

                    if(!depElements.Contains(SelectedTagType)) {
                        TagPointFinder pathFinder = new TagPointFinder(room);
                        UV point = pathFinder.GetPointByPlacementWay(positionPlacementWay, activeView);

                        LocationPoint roomLocation = (LocationPoint) room.Location;
                        XYZ testPoint = new XYZ(point.U, point.V, roomLocation.Point.Z);

                        if(!room.IsPointInRoom(testPoint)) point = pathFinder.GetPointByPath();

                        var newTag = Document.Create.NewRoomTag(new LinkElementId(room.Id), point, activeView.Id);
                        newTag.ChangeTypeId(SelectedTagType);
                    }
                }
                t.Commit();
            }
        }
    }
}
