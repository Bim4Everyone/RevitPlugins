using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

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
            return new FilteredElementCollector(Document)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .Select(element => element.Category)
                .Where(category => category != null && HasMarkParameter(category))
                .Distinct()
                .ToList();
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
                return line.Project(center)?.Distance < 13.3;
            }).ToList();
        }

        private bool HasMarkParameter(Category category) {
            var collector = new FilteredElementCollector(Document)
                .OfCategoryId(category.Id)
                .WhereElementIsNotElementType()
                .FirstOrDefault();

            if(collector != null) {
                var param = collector.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
                return param != null;
            }

            return false;
        }

        public Transaction CreateTransaction(string transactionName) {
            return new Transaction(Document, transactionName);
        }

        public Category GetCategoryById(ElementId categoryId) {
            return Document.Settings.Categories
                .Cast<Category>()
                .FirstOrDefault(cat => cat.Id == categoryId);
        }

        public List<Element> GetElements(Category category) {
            if(category == null) {
                return new List<Element>();
            }

            FilteredElementCollector collector = new FilteredElementCollector(Document);

            return collector
                .WherePasses(new ElementCategoryFilter(category.Id))
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();
        }

        public List<CurveElement> GetLinesAndSplines() {
            FilteredElementCollector collector = new FilteredElementCollector(Document);

            return collector
                .OfClass(typeof(CurveElement))
                .Cast<CurveElement>()
                .Where(curveElement =>
                    curveElement.GeometryCurve != null &&
                    (curveElement is ModelLine || curveElement is ModelNurbSpline)).ToList();
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
