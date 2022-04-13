using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitWindowGapPlacement.Model {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public Application Application => UIApplication.Application;


        public Document Document => UIDocument.Document;
        public UIDocument UIDocument => UIApplication.ActiveUIDocument;

        public Element GetNearestElement(Wall hostElement) {
            var bbFilter = new BoundingBoxIntersectsFilter(GetOutline(hostElement));
            var walls = new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Walls)
                .WherePasses(bbFilter)
                .OfType<Wall>()
                .ToList();

            if(walls.Count == 1) {
                return walls[0];
            }

            walls = walls
                .Where(item => IsParallelWalls(hostElement, item))
                .ToList();
            
            if(walls.Count == 1) {
                return walls[0];
            }

            throw new Exception("Шо?");
        }

        private bool IsParallelWalls(Wall first, Wall second) {
            double angle = first.Orientation.AngleTo(second.Orientation);
            return Math.Abs(angle) < 0.001 || Math.Abs(angle - 180) < 0.001;
        }

        public List<ICanPlaceWindowGap> GetPlaceableElements() {
            var walls = GetCurtainWalls()
                .Select(item => new CurtainWallWindow(item, this));

            var windows = GetWindows()
                .Select(item => new BasicWindow(item, this));

            return Enumerable.Empty<ICanPlaceWindowGap>()
                .Union(walls)
                .Union(windows)
                .ToList();
        }

        public List<FamilyInstance> GetWindows() {
            return GetAllWindows()
                .Where(item => IsWindow(item))
                .ToList();
        }

        public IList<Wall> GetCurtainWalls() {
            return new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_Walls)
                .OfType<Wall>()
                .Where(item => item.WallType.Kind == WallKind.Curtain)
                .ToList();
        }

        public List<FamilyInstance> GetWindowGaps() {
            return GetAllWindows()
                .Where(item => IsWindowGap(item))
                .ToList();
        }

        public FamilySymbol GetWindowGapType() {
            return new FilteredElementCollector(Document)
                .WhereElementIsElementType()
                .OfCategory(BuiltInCategory.OST_Windows)
                .OfType<FamilySymbol>()
                .FirstOrDefault(item => item.Name.Equals("Окн_Проём_Прямоугольный"));
        }

        public void PlaceWindowGaps() {
            using(Transaction transaction = Document.StartTransaction("Расстановка проемов окон")) {
                // Удаляем все расставленные до этого проемы окон
                RemoveWindowGaps();

                FamilySymbol windowGapType = GetWindowGapType();

                // Заново расставляем проемы окон
                foreach(ICanPlaceWindowGap placeableElement in GetPlaceableElements()) {
                    placeableElement.PlaceWindowGap(Document, windowGapType);
                }

                transaction.Commit();
            }
        }

        private IList<FamilyInstance> GetAllWindows() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Windows)
                .OfType<FamilyInstance>()
                .ToList();
        }

        private bool IsWindow(FamilyInstance familyInstance) {
            return familyInstance.Host is Wall && familyInstance.HostFace == null;
        }

        private bool IsWindowGap(FamilyInstance familyInstance) {
            return familyInstance.Host is Wall && familyInstance.HostFace != null;
        }

        private void RemoveWindowGaps() {
            ElementId[] windowGaps = GetWindowGaps()
                .Select(item => item.Id)
                .ToArray();

            Document.Delete(windowGaps);
        }

        private Outline GetOutline(Element element) {
            var boundingBox = element.get_BoundingBox(null);
            if(boundingBox == null) {
                return new Outline(XYZ.Zero, XYZ.Zero);
            }

            var offset = new XYZ(50, 50, 50);
            return new Outline(boundingBox.Min - offset, boundingBox.Max + offset);
        }
    }
}