using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitMarkingElements.Models {
    internal class RevitRepository {

        private readonly ILocalizationService _localizationService;
        public RevitRepository(UIApplication uiApplication, ILocalizationService localizationService) {
            UIApplication = uiApplication;
            _localizationService = localizationService;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public List<Category> GetCategoriesWithMarkParam(BuiltInParameter markParam) {

            List<Category> categories = new List<Category>();

            categories = new FilteredElementCollector(Document)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Where(element => element.IsExistsParam(markParam))
                .Select(element => element.Category)
                .Where(category => category != null)
                .GroupBy(category => category.Id)
                .Select(group => group.First())
                .ToList();

            return categories;
        }

        public List<Element> GetElementsIntersectingLine(List<Element> elements, Curve lineElement) {
            List<Element> intersectingElements = new List<Element>();

            foreach(var element in elements) {
                GeometryElement geometryElement = element.get_Geometry(new Options());
                if(geometryElement == null)
                    continue;

                bool isIntersecting = false;

                foreach(GeometryObject geometryObject in geometryElement) {
                    if(geometryObject is GeometryInstance geometryInstance) {
                        GeometryElement instanceGeometry = geometryInstance.GetInstanceGeometry();

                        foreach(GeometryObject instanceGeoObj in instanceGeometry) {
                            if(instanceGeoObj is Solid solid) {
                                if(solid.Faces.IsEmpty) {
                                    continue;
                                }
                                Transform flattenTransform = Transform.CreateTranslation(new XYZ(0, 0, -solid.ComputeCentroid().Z));
                                Solid flattenedSolid = SolidUtils.CreateTransformed(solid, flattenTransform);

                                SolidCurveIntersectionOptions options = new SolidCurveIntersectionOptions {
                                    ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
                                };

                                SolidCurveIntersection intersection = flattenedSolid.IntersectWithCurve(lineElement, options);

                                if(intersection.SegmentCount > 0) {
                                    for(int i = 0; i < intersection.SegmentCount; i++) {
                                        Curve intersectedSegment = intersection.GetCurveSegment(i);

                                        if(intersectedSegment.Length > 0.1) {
                                            isIntersecting = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if(instanceGeoObj is Line line) {
                                XYZ elementStart = new XYZ(line.GetEndPoint(0).X, line.GetEndPoint(0).Y, 0);
                                XYZ elementEnd = new XYZ(line.GetEndPoint(1).X, line.GetEndPoint(1).Y, 0);

                                if(elementStart.DistanceTo(elementEnd) < 0.1) {
                                    continue;
                                }

                                Line projectedElementLine = Line.CreateBound(elementStart, elementEnd);
                                SetComparisonResult result = lineElement.Intersect(projectedElementLine);
                                if(result == SetComparisonResult.Overlap || result == SetComparisonResult.Subset) {
                                    isIntersecting = true;
                                }
                            }
                        }
                    }
                }

                if(isIntersecting) {
                    intersectingElements.Add(element);
                }
            }

            return intersectingElements;
        }


        public List<CurveElement> SelectLinesOnView() {
            var selectedLines = new List<CurveElement>();
            ISelectionFilter lineSelectionFilter = new CurveElementSelectionFilter();
            var finishLineSelection = _localizationService.GetLocalizedString("MainWindow.FinishLineSelection");
            var isDone = false;

            //Данная реализация была выбрана для того что бы получить объекты именно в том порядке
            //в котором они были выбраны на виде. PickObjects не подходит т.к. возвращает элементы в порядке их размещения на виде
            while(!isDone) {
                try {
                    var reference = ActiveUIDocument.Selection.PickObject(
                        ObjectType.Element,
                        lineSelectionFilter,
                        finishLineSelection);

                    if(reference != null) {
                        var element = ActiveUIDocument.Document.GetElement(reference.ElementId) as CurveElement;
                        if(element != null && element.GeometryCurve != null) {
                            selectedLines.Add(element);
                        }
                    }
                } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
                    isDone = true;
                }
            }

            return selectedLines.Distinct(new CurveElementComparer()).ToList();
        }

        public List<Element> GetElements(ElementId categoryId) {
            if(categoryId == null) {
                return new List<Element>();
            }

            FilteredElementCollector collector = new FilteredElementCollector(Document);

            return collector
                .WherePasses(new ElementCategoryFilter(categoryId))
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();
        }

        public List<ElementId> GetSelectedElementsIds() {
            var selectedElementIds = ActiveUIDocument.Selection.GetElementIds();

            if(selectedElementIds == null || !selectedElementIds.Any()) {
                return new List<ElementId>();
            }

            return selectedElementIds.ToList();
        }

        public XYZ GetElementCoordinates(Element element) {
            var locationPoint = element.Location as LocationPoint;
            if(locationPoint != null) {
                return locationPoint.Point;
            }

            var locationCurve = element.Location as LocationCurve;
            if(locationCurve != null) {
                return locationCurve.Curve.Evaluate(0.5, true);
            }

            return null;
        }
    }
}
