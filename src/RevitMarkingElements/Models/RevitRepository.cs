using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitMarkingElements.ViewModels;

namespace RevitMarkingElements.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public List<Category> GetCategoriesWithMarkParam() {

            var markParam = MainViewModel.MarkParam;
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
                var lineToObjectDistance = 13.3;
                return line.Project(center)?.Distance < lineToObjectDistance;
            }).ToList();
        }

        public Transaction CreateTransaction(string transactionName) {
            return new Transaction(Document, transactionName);
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

        public List<CurveElement> GetLinesAndSplines() {
            var selectedElementIds = ActiveUIDocument.Selection.GetElementIds();

            if(!selectedElementIds.Any()) {
                return new List<CurveElement>();
            }

            return selectedElementIds
                .Select(id => Document.GetElement(id))
                .OfType<CurveElement>()
                .Where(curveElement =>
                    curveElement.GeometryCurve != null &&
                    (curveElement is ModelLine || curveElement is ModelNurbSpline))
                .Reverse()
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
