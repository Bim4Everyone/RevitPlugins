using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Security;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitRoomTagPlacement.ViewModels;

using Document = Autodesk.Revit.DB.Document;
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

        public List<RoomFromRevit> GetSelectedRooms() {
            return ActiveUIDocument.GetSelectedElements()
                .Where(x => x is Room)
                .OfType<Room>()
                .Select(x => new RoomFromRevit(x))
                .ToList();
        }

        public List<RoomFromRevit> GetRoomsOnActiveView() {
            List<RevitLinkInstance> links = new FilteredElementCollector(Document)
                .OfClass(typeof(RevitLinkInstance))
                .ToElements()
                .OfType<RevitLinkInstance>()
                .Where(x => x.GetLinkDocument() != null)
                .ToList();

            List<RoomFromRevit> allRooms = new FilteredElementCollector(Document, Document.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .OfType<Room>()
                .Select(x => new RoomFromRevit(x))
                .ToList();

            foreach(var link in links) {
                List<RoomFromRevit> rooms = new FilteredElementCollector(link.GetLinkDocument())
                .OfCategory(BuiltInCategory.OST_Rooms)
                .OfType<Room>()
                .Select(x => new RoomFromRevit(x, link.Id))
                .ToList();

                allRooms.AddRange(rooms);
            }

            return allRooms;
        }

        public List<RoomTagTypeModel> GetRoomTags() {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(FamilySymbol))
                .OfType<RoomTagType>()
                .Select(x => new RoomTagTypeModel(x))
                .ToList();
        }

        public ObservableCollection<string> GetRoomNames(IList<RoomGroupViewModel> roomGroups) {
            var selectedAparts = roomGroups.Where(x => x.IsChecked).SelectMany(x => x.Apartments);
            IEnumerable<string> uniqueNames = new List<string>();

            if(selectedAparts.Count() > 0) {
                uniqueNames = new List<string>(selectedAparts.First().RoomNames);

                foreach(var group in selectedAparts) {
                    uniqueNames = uniqueNames.Intersect(group.RoomNames);
                }
            }

            return new ObservableCollection<string>(uniqueNames);
        }

        public List<RoomFromRevit> FilterRoomsForPlacement(IList<RoomGroupViewModel> roomGroups,
                                                        GroupPlacementWay groupPlacementWay,
                                                        string roomName = "") {
            var selectedAparts = roomGroups
                .Where(x => x.IsChecked)
                .SelectMany(x => x.Apartments)
                .ToList();

            if(groupPlacementWay == GroupPlacementWay.EveryRoom) {
                return selectedAparts
                    .SelectMany(x => x.Rooms)
                    .ToList();
            } 
            else if(groupPlacementWay == GroupPlacementWay.OneRoomPerGroupRandom) {
                return selectedAparts
                    .Select(x => x.MaxAreaRoom)
                    .ToList();
            } 
            else if(groupPlacementWay == GroupPlacementWay.OneRoomPerGroupByName) {
                return selectedAparts
                    .Select(x => x.GetRoomByName(roomName))
                    .ToList();
            } 
            else {
                return new List<RoomFromRevit>();
            }
        }

        public void PlaceTagsByPositionAndGroup(IList<RoomGroupViewModel> roomGroups, 
                                            ElementId selectedTagType,
                                            GroupPlacementWay groupPlacementWay,
                                            PositionPlacementWay positionPlacementWay,
                                            double indent,
                                            string roomName = "") {

            List<RoomFromRevit> rooms = FilterRoomsForPlacement(roomGroups, groupPlacementWay, roomName);

            View activeView = Document.ActiveView;
            ElementOwnerViewFilter viewFilter = new ElementOwnerViewFilter(activeView.Id);
            double indentFeet = ConvertIndentToFeet(indent);

            using(Transaction t = Document.StartTransaction("Маркировать помещения")) {
                foreach(var room in rooms) {
                    var depElements = room.RoomObject
                        .GetDependentElements(viewFilter)
                        .Select(x => Document.GetElement(x))
                        .Where(x => x != null)
                        .Select(x => x.GetTypeId())
                        .ToList();

                    if(!depElements.Contains(selectedTagType)) {
                        TagPointFinder pathFinder = new TagPointFinder(room.RoomObject, indentFeet);
                        UV point = pathFinder.GetPointByPlacementWay(positionPlacementWay, activeView);

                        Location roomLocation = room.RoomObject.Location;

                        LocationPoint roomLocationPoint = (LocationPoint) roomLocation;
                        XYZ testPoint = new XYZ(point.U, point.V, room.CenterPoint.Z);

                        if(!room.RoomObject.IsPointInRoom(testPoint)) point = pathFinder.GetPointByPath();

                        /* Невозможно отфильтровать помещения из связанного файла для активного вида.
                           Способ получения помещений через CustomExporter не работает, так как помещения не экспортируются.
                           В качестве решения принято брать все помещения из связанного файла и размещать марку на каждом.
                           В таком случае все марки размещаются в проекте, но если помещение отсутствует на виде, то марка не отображается.
                           Для удаления марок, которые не отображаются, скрипт пытается получить BoundingBox для каждой марки, 
                           если он null, то марка удаляется.                         
                           */

                        RoomTag newTag;

                        if(room.LinkId == null) {
                            newTag = Document.Create.NewRoomTag(new LinkElementId(room.RoomObject.Id), point, activeView.Id);
                        } 
                        else {
                            newTag = Document.Create.NewRoomTag(new LinkElementId(room.LinkId, room.RoomObject.Id), point, activeView.Id);
                        }

                        if(newTag?.get_BoundingBox(activeView) == null) {
                            Document.Delete(newTag.Id);
                        }
                        else if(newTag != null) { 
                            newTag.ChangeTypeId(selectedTagType);
                        }
                    }
                }
                t.Commit();
            }
        }

#if REVIT_2020_OR_LESS
        private double ConvertIndentToFeet(double indent) {
            return UnitUtils.ConvertToInternalUnits(indent, DisplayUnitType.DUT_MILLIMETERS);
        }
#else
        private double ConvertIndentToFeet(double indent) {
            return UnitUtils.ConvertToInternalUnits(indent, UnitTypeId.Millimeters);
        }
#endif
    }
}
