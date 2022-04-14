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

        public IEnumerable<HostObject> GetNearestElements(HostObject host, XYZ point, XYZ direction) {
            ReferenceIntersector refIntersection = GetReferenceIntersector(host);

            var positiveNearestElement
                = refIntersection.FindNearest(point, direction);

            var negativeNearestElement
                = refIntersection.FindNearest(point, -direction);

            var refNearestElement = new[] {
                    new {Direction = direction, NearestElement = positiveNearestElement},
                    new {Direction = -direction, NearestElement = negativeNearestElement}
                }
                .Where(item => item.NearestElement != null)
                .Where(item => item.NearestElement.Proximity < 3.15) // один метр
                .OrderBy(item => item.NearestElement.Proximity)
                .FirstOrDefault();

            if(refNearestElement == null) {
                yield break;
            }

            ReferenceWithContext context
                = refNearestElement.NearestElement;

            host = (HostObject) Document.GetElement(context.GetReference().ElementId);
            yield return host;

            while(context != null) {
                refIntersection = GetReferenceIntersector(host);

                point = point + refNearestElement.Direction * refNearestElement.NearestElement.Proximity;
                context = refIntersection.FindNearest(point, refNearestElement.Direction);

                if(context != null) {
                    host = (HostObject) Document.GetElement(context.GetReference().ElementId);
                    yield return host;
                }
            }
        }

        private ReferenceIntersector GetReferenceIntersector(Element host) {
            var exclusionFilter =
                new ExclusionFilter(new[] {host.Id});

            var categoryFilter =
                new ElementCategoryFilter(BuiltInCategory.OST_Walls);

            var andFilter
                = new LogicalAndFilter(new ElementFilter[] {exclusionFilter, categoryFilter});

            return new ReferenceIntersector(andFilter, FindReferenceTarget.All, (View3D) Document.ActiveView);
        }

        public XYZ GetPlaceLocation(Element wall) {
            LocationCurve location = (LocationCurve) wall.Location;
            Line line = (Line) location.Curve;
            double offset = wall.GetParamValue<double>(BuiltInParameter.WALL_BASE_OFFSET);
            double height = wall.GetParamValue<double>(BuiltInParameter.WALL_USER_HEIGHT_PARAM);

            return line.Origin - ((line.GetEndPoint(0) - line.GetEndPoint(1)) / 2) + new XYZ(0, 0, height + offset);
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
            return !IsWindow(familyInstance);
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