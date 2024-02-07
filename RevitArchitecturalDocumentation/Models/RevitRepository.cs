using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

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


        public Regex RegexForBuildingPart { get; set; } = new Regex(@"К(.*?)_");
        public Regex RegexForBuildingSection { get; set; } = new Regex(@"С(.*?)$");
        public Regex RegexForBuildingSectionPart { get; set; } = new Regex(@"часть (.*?)$");
        public Regex RegexForLevel { get; set; } = new Regex(@"^(.*?) ");

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
                .OrderBy(a => a.Name)
                .ToList();

        public List<ElementType> ViewportTypes => new FilteredElementCollector(Document)
                .OfClass(typeof(ElementType))
                .OfType<ElementType>()
                .Where(a => a.get_Parameter(BuiltInParameter.VIEWPORT_ATTR_SHOW_EXTENSION_LINE) != null)
                .OrderBy(a => a.Name)
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

        public ViewPlan FindViewByName(string viewName) => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewPlan))
                .OfType<ViewPlan>()
                .FirstOrDefault(o => o.Name.Equals(viewName));

        public ViewSchedule GetSpecByName(string specName) => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSchedule))
                .OfType<ViewSchedule>()
                .FirstOrDefault(o => o.Name.Equals(specName));

        /// <summary>
        /// Собирает все видовые экраны спецификаций на листе и возвращает ту, что имеет запрошенное имя или null
        /// </summary>
        public ScheduleSheetInstance GetSpecFromSheetByName(ViewSheet viewSheet, string specName) =>
            viewSheet.GetDependentElements(new ElementClassFilter(typeof(ScheduleSheetInstance)))
                .Select(id => Document.GetElement(id) as ScheduleSheetInstance)
                .FirstOrDefault(v => v.Name == specName);


        /// <summary>
        /// Получает виды в плане, выбранных до запуска плагина.
        /// </summary>
        public ObservableCollection<ViewPlan> GetSelectedViewPlans() => new ObservableCollection<ViewPlan>(ActiveUIDocument.Selection.GetElementIds()
            .Select(id => Document.GetElement(id) as ViewPlan)
            .Where(v => v != null));
    }
}
