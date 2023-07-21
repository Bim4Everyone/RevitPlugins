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
                .Select(x => new RoomElement((Room)x))
                .ToList();
        }

        public IList<RoomElement> GetRoomsOnActiveView() {
            return new FilteredElementCollector(Document, Document.ActiveView.Id)
                .WhereElementIsNotElementType()
                .OfClass(typeof(SpatialElement))
                .Select(x => new RoomElement((Room) x))
                .ToList();
        }

        public IList<RoomElement> GetSelectedRooms() {
            return ActiveUIDocument.GetSelectedElements()
                .OfType<Room>()
                .Select(x => new RoomElement(x))
                .ToList();
        }
    }
}