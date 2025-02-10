using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.Revit.Geometry;
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
            var selectedElements = GetSelectedElements();

            var categories = selectedElements
                .OfType<FamilyInstance>()
                .Where(element => element.IsExistsParam(markParam))
                .Select(element => element.Category)
                .Where(category => category != null)
                .GroupBy(category => category.Id)
                .Select(group => group.First())
                .ToList();

            return categories;
        }

        public List<Element> GetElementsIntersectingLine(List<Element> elements, Curve lineElement) {
            var intersectingElements = new List<Element>();

            foreach(var element in elements) {
                var solids = element.GetSolids().Where(s => s.Volume > 0).ToList();

                if(solids.Count == 0) {
                    var boundingBox = element.get_BoundingBox(null);
                    if(boundingBox != null) {
                        var boundingBoxSolid = boundingBox.CreateSolid();
                        if(IsLineIntersectingSolid(lineElement, boundingBoxSolid)) {
                            intersectingElements.Add(element);
                            continue;
                        }
                    }
                } else {
                    if(solids.Any(solid => IsLineIntersectingSolid(lineElement, solid))) {
                        intersectingElements.Add(element);
                        continue;
                    }
                }
            }

            return intersectingElements;
        }

        private bool IsLineIntersectingSolid(Curve line, Solid solid) {
            var offsetTransform = Transform.CreateTranslation(new XYZ(0, 0, -solid.ComputeCentroid().Z));
            var offsetSolid = SolidUtils.CreateTransformed(solid, offsetTransform);

            var options = new SolidCurveIntersectionOptions {
                ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
            };

            var intersection = offsetSolid.IntersectWithCurve(line, options);

            return intersection.SegmentCount > 0 && intersection.GetCurveSegment(0).Length > 0.1;
        }

        public List<CurveElement> SelectLinesOnView() {
            var selectedLines = new List<CurveElement>();
            var lineSelectionFilter = new CurveElementSelectionFilter();
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
                        var element = Document.GetElement(reference.ElementId) as CurveElement;
                        if(element != null && element.GeometryCurve != null) {
                            selectedLines.Add(element);
                        }
                    }
                } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
                    isDone = true;
                }
            }

            var uniqueLines = new List<CurveElement>();
            var orderedIds = new List<ElementId>();

            foreach(var line in selectedLines) {
                if(!orderedIds.Contains(line.Id)) {
                    orderedIds.Add(line.Id);
                    uniqueLines.Add(line);
                }
            }

            return uniqueLines;
        }
        public List<Element> GetElementsByCategory(ElementId categoryId) {
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

        public List<Element> GetSelectedElements() {
            var selectedElementIds = ActiveUIDocument.Selection.GetElementIds();

            if(selectedElementIds == null || !selectedElementIds.Any()) {
                return new List<Element>();
            }

            var selectedElements = selectedElementIds
                 .Select(id => Document.GetElement(id))
                 .Where(element => element != null)
                 .ToList();

            return selectedElements;
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
