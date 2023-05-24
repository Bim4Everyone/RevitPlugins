using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitCreatingFiltersByValues.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public List<ElementId> FilterableCategories => ParameterFilterUtilities.GetAllFilterableCategories().ToList();
        public List<Element> ElementsInView => new FilteredElementCollector(Document, Document.ActiveView.Id)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

        public List<FillPatternElement> AllPatterns => new FilteredElementCollector(Document)
                .OfClass(typeof(FillPatternElement))
                .OfType<FillPatternElement>()
                .ToList();
        

    }
}