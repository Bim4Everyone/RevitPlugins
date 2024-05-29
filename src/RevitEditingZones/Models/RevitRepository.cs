using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitEditingZones.Models {
    internal class RevitRepository {
        public static readonly string AreaSchemeName = "Назначение этажа СМР";

        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
            AreaScheme = GetAreaScheme();
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public AreaScheme AreaScheme { get; }

        public bool IsKoordFile() {
            return Document.Title.Contains("_KOORD");
        }

        public bool IsKoordFile(RevitLinkType revitLinkType) {
            return revitLinkType.Name.Contains("_KOORD");
        }

        public Transaction StartTransaction(string transactionName) {
            return Document.StartTransaction(transactionName);
        }

        public AreaScheme GetAreaScheme() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_AreaSchemes)
                .OfType<AreaScheme>()
                .FirstOrDefault(item => item.Name.Equals(AreaSchemeName));
        }

        public IEnumerable<ViewPlan> GetAreaPlanes() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(ViewPlan))
                .OfType<ViewPlan>()
                .Where(item => IsAreaPlan(item));
        }

        public IEnumerable<Level> GetLevels() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Level))
                .OfType<Level>();
        }

        public bool IsAreaPlan() {
            return IsAreaPlan(Document.ActiveView);
        }

        public bool IsAreaPlan(View view) {
            if(view is ViewPlan viewPlan) {
                return viewPlan.AreaScheme != null
                       && viewPlan.AreaScheme.Id == AreaScheme.Id;
            }

            return false;
        }

        public bool HasCorruptedAreas() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Areas)
                .OfType<Area>()
                .Any(item => item.IsNotEnclosed() || item.IsRedundant());
        }

        public bool HasAreaScheme() {
            return GetAreaScheme() != null;
        }

        public IEnumerable<Area> GetAreas(ViewPlan areaPlan) {
            var areaFilter = new AreaFilter(AreaScheme);
            return new FilteredElementCollector(Document, areaPlan.Id)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Areas)
                .OfType<Area>()
                .Where(item => areaFilter.AllowElement(item));
        }

        public Level GetLevel(Area area) {
            var fixCommentParam = SharedParamsConfig.Instance.FixComment;
            if(area.IsExistsParamValue(fixCommentParam)) {
                var paramValue = area.GetParamValue<string>(fixCommentParam);
#if REVIT_2023_OR_LESS
                var levelId = int.TryParse(paramValue, out int elementId)
                    ? new ElementId(elementId)
                    : ElementId.InvalidElementId;
#else
                var levelId = long.TryParse(paramValue, out long elementId)
                    ? new ElementId(elementId)
                    : ElementId.InvalidElementId;
#endif
                return Document.GetElement(levelId) as Level;
            }

            return null;
        }

        public void UpdateAreaName(Area area, string areaName) {
            area.SetParamValue(BuiltInParameter.ROOM_NAME, areaName);
        }

        public void UpdateAreaLevel(Area area, Level level) {
            area.SetParamValue(SharedParamsConfig.Instance.FixComment, level?.Id.ToString());
        }
    }

    internal class AreaFilter : ISelectionFilter {
        private readonly AreaScheme _areaScheme;

        public AreaFilter(AreaScheme areaScheme) {
            _areaScheme = areaScheme;
        }

        public bool AllowElement(Element elem) {
            var area = elem as Area;
            return area != null && _areaScheme?.Id == area.AreaScheme.Id;
        }

        public bool AllowReference(Reference reference, XYZ position) {
            return false;
        }
    }

    internal class FloorData {
        public FloorData(string floorName) {
            string[] split = floorName.Split('_');
            FloorName = split.ElementAtOrDefault(0);
            BlockTypeName = split.ElementAtOrDefault(1);
            Elevation = split.ElementAtOrDefault(2);
        }

        public string FloorName { get; }
        public string BlockTypeName { get; }
        public string Elevation { get; }
    }
}