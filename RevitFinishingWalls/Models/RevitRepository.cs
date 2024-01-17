using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace RevitFinishingWalls.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;


        public ICollection<WallType> GetWallTypes() {
            return new FilteredElementCollector(Document)
                .WhereElementIsElementType()
                .OfClass(typeof(WallType))
                .Cast<WallType>()
                .Where(wallType => wallType.Kind == WallKind.Basic)
                .ToHashSet();
        }

        public ICollection<Room> GetSelectedRooms() {
            return ActiveUIDocument.Selection
                .GetElementIds()
                .Select(id => Document.GetElement(id))
                .Where(element => element is Room room && room.Area > 0)
                .Cast<Room>()
                .ToHashSet();
        }
    }
}
