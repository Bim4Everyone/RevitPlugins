using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitReinforcementCoefficient.Models {
    internal class RevitRepository {

        private readonly LogicalOrFilter _categoriesFilter = new LogicalOrFilter(
            new List<ElementFilter>() {
                new ElementCategoryFilter(BuiltInCategory.OST_Walls),
                new ElementCategoryFilter(BuiltInCategory.OST_Floors),
                new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming),
                new ElementCategoryFilter(BuiltInCategory.OST_Columns),
                new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns),
                new ElementCategoryFilter(BuiltInCategory.OST_StructuralFoundation)
            });


        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public List<Element> ElementsByFilter => new FilteredElementCollector(Document, Document.ActiveView.Id)
            .WherePasses(_categoriesFilter)
            .ToList();

        public List<Element> RebarsInActiveView => new FilteredElementCollector(Document, Document.ActiveView.Id)
            .OfCategory(BuiltInCategory.OST_Rebar)
            .ToList();


    }
}
