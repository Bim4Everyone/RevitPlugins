using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitExamplePlugin.Models {
    internal class WallRevitRepository {
        public WallRevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public XYZ PickPoint(string statusPrompt) {
            return ActiveUIDocument.Selection.PickPoint(statusPrompt);
        }

        public IEnumerable<Wall> GetWalls() {
            return new FilteredElementCollector(Document, Document.ActiveView.Id)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .OfType<Wall>();
        }

        public IEnumerable<WallType> GetWallTypes() {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(WallType))
                .WhereElementIsElementType()
                .OfType<WallType>();
        }

        public Wall CreateWall(CustomLocation location, WallType wallType, double height) {
            Curve curve = Line.CreateBound(location.Start, location.Finish);
            return Wall.Create(Document,
                curve, wallType.Id, GetLevel().Id, height, 0, false, false);
        }

        /// <summary>
        /// Возвращает True, если активный вид - план, иначе False
        /// </summary>
        public bool ActiveViewIsPlan() {
            return Document.ActiveView as ViewPlan != null;
        }

        private Level GetLevel() {
            return Document.ActiveView.GenLevel;
        }
    }
}
