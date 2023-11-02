using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitArchitecturalDocumentation.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;
        public List<Element> VisibilityScopes => new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_VolumeOfInterest)
            .ToList();

        public List<Level> Levels => new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_Levels)
            .OfType<Level>()
            .ToList();

        public List<ViewFamilyType> ViewFamilyTypes => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewFamilyType))
                .OfType<ViewFamilyType>()
                .Where(a => ViewFamily.FloorPlan == a.ViewFamily)
                .ToList();

        public List<ElementType> ViewportTypes => new FilteredElementCollector(Document)
                .OfClass(typeof(ElementType))
                .OfType<ElementType>()
                .Where(a => a.get_Parameter(BuiltInParameter.VIEWPORT_ATTR_SHOW_EXTENSION_LINE) != null)
                .ToList();

        public List<FamilySymbol> TitleBlocksInProject => new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsElementType()
                .OfType<FamilySymbol>()
                .ToList();


        public ViewSheet GetSheetByName(string sheetName) => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSheet))
                .OfType<ViewSheet>()
                .FirstOrDefault(o => o.Name.Equals(sheetName));

        public ViewPlan GetViewByName(string viewName) => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewPlan))
                .OfType<ViewPlan>()
                .FirstOrDefault(o => o.Name.Equals(viewName));

        public ViewSchedule GetSpecByName(string specName) => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSchedule))
                .OfType<ViewSchedule>()
                .FirstOrDefault(o => o.Name.Equals(specName));

        public ScheduleSheetInstance GetSpecFromSheetByName(ViewSheet viewSheet, string specName) => viewSheet.GetDependentViewIds()
            .Select(id => Document.GetElement(id) as ScheduleSheetInstance)
            .Where(v => v != null)
            .FirstOrDefault(v => v.Name == specName);
    }
}