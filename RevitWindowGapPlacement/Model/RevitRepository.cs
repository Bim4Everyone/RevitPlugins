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

        public Wall GetNearestElement(Wall host, XYZ point, XYZ direction) {
            var exclusionFilter =
                new ExclusionFilter(new[] {host.Id});

            var categoryFilter =
                new ElementCategoryFilter(BuiltInCategory.OST_Walls);

            var andFilter
                = new LogicalAndFilter(new ElementFilter[] {exclusionFilter, categoryFilter});

            var refIntersection =
                new ReferenceIntersector(andFilter, FindReferenceTarget.All, (View3D) Document.ActiveView);

            var positiveNearestElement
                = refIntersection.FindNearest(point, direction);

            var negativeNearestElement
                = refIntersection.FindNearest(point, -direction);

            var nearestElement = new[] {positiveNearestElement, negativeNearestElement}
                .Where(item => item != null)
                .OrderByDescending(item => positiveNearestElement.Proximity)
                .FirstOrDefault();

            if(nearestElement == null) {
                return null;
            }

            return (Wall) Document.GetElement(nearestElement.GetReference().ElementId);
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

        public FamilySymbol GetWindowGapType(string ss) {
            return new FilteredElementCollector(Document)
                .WhereElementIsElementType()
                .OfCategory(BuiltInCategory.OST_Windows)
                .OfType<FamilySymbol>()
                .FirstOrDefault(item => item.Name.Equals(ss));
        }

        public void PlaceWindowGaps() {
            using(Transaction transaction = Document.StartTransaction("Расстановка проемов окон")) {
                // Удаляем все расставленные до этого проемы окон
                RemoveWindowGaps();


                Document.Regenerate();

                FamilySymbol windowGapType = GetWindowGapType();
                if(!windowGapType.IsActive) {
                    windowGapType.Activate();
                }

                // Заново расставляем проемы окон
                foreach(ICanPlaceWindowGap placeableElement in GetPlaceableElements()) {
                    try {
                        placeableElement.PlaceWindowGap(Document, windowGapType);
                    } catch { }
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

            var offset = new XYZ(0.1, 0.1, 0.1);
            return new Outline(boundingBox.Min - offset, boundingBox.Max + offset);
        }
    }
}