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

        public List<Element> GetElementsIntersectingLine(List<Element> elements, CurveElement lineElement) {
            var line = lineElement.GeometryCurve;
            if(line == null) {
                return new List<Element>();
            }

            return elements.Where(element => {
                var bbox = element.get_BoundingBox(null);
                if(bbox == null)
                    return false;

                var center = (bbox.Min + bbox.Max) / 2.0;
                // расстояние когда линия визуально и в действительности пересекает объект
                var lineToObjectDistance = 13.3;
                return line.Project(center)?.Distance < lineToObjectDistance;
            }).ToList();
        }

        public List<CurveElement> SelectLinesOnView() {
            var selectedLines = new List<CurveElement>();
            ISelectionFilter lineSelectionFilter = new CurveElementSelectionFilter();
            var finishLineSelection = _localizationService.GetLocalizedString("MainWindow.FinishLineSelection");
            var isDone = false;


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

        public Transaction CreateTransaction(string transactionName) {
            return new Transaction(Document, transactionName);
        }

        public List<Element> GetElements(ElementId categoryId) {
            if(categoryId == null) {
                return new List<Element>();
            }

            var selectedElementIds = ActiveUIDocument.Selection.GetElementIds();

            if(selectedElementIds == null || !selectedElementIds.Any()) {
                return new List<Element>();
            }

            FilteredElementCollector collector = new FilteredElementCollector(Document);

            return collector
                .WherePasses(new ElementCategoryFilter(categoryId))
                .WhereElementIsNotElementType()
                .Where(element => selectedElementIds.Contains(element.Id))
                .ToList();
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
