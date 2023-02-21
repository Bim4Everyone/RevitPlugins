using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;

namespace RevitCopingZones.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public Transaction StartTransaction(string transactionName) {
            return Document.StartTransaction(transactionName);
        }

        public AreaScheme GetAreaScheme() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_AreaSchemes)
                .OfType<AreaScheme>()
                .FirstOrDefault(item => item.Name.Equals("Назначение этажа СМР"));
        }

        public IEnumerable<FloorPlan> GetFloorPlans() {
            var floorPlans = new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Level))
                .OfType<Level>()
                .GroupBy(item => item.Name.Split('_').First())
                .Select(item => new FloorPlan(item.Key, item))
                .ToDictionary(item => item.Name);

            var areaPlanes = GetAreaPlanes();
            foreach(ViewPlan areaPlan in areaPlanes) {
                if(floorPlans.TryGetValue(areaPlan.Name, out FloorPlan floorPlan)) {
                    floorPlan.AreaPlan = areaPlan;
                    floorPlan.HasAreasInPlan = HasAreas(areaPlan);
                }
            }

            return floorPlans.Values;
        }

        public IEnumerable<ViewPlan> GetAreaPlanes() {
            var areaScheme = GetAreaScheme();
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(ViewPlan))
                .OfType<ViewPlan>()
                .Where(item => item.AreaScheme != null)
                .Where(item => item.AreaScheme.Id == areaScheme?.Id);
        }

        public bool HasAreas(ViewPlan areaPlan) {
            return GetAreas(areaPlan).Any();
        }

        public IEnumerable<Area> GetCorruptedAreas() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Areas)
                .OfType<Area>()
                .Where(item => item.IsNotEnclosed() || item.IsRedundant());
        }

        public IEnumerable<Area> GetAreas(ViewPlan areaPlan) {
            return new FilteredElementCollector(Document, areaPlan.Id)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Areas)
                .OfType<Area>();
        }

        public IEnumerable<Area> GetSelectedAreas() {
            return new UIDocument(Document).Selection
                .PickElementsByRectangle(new AreaFilter(GetAreaScheme()),
                    "Выберите зоны на текущем виде")
                .OfType<Area>();
        }

        public IEnumerable<Area> CopyAreaToView(FloorPlan floorPlan, IEnumerable<Area> copingAreas) {
            return CopyAreaToView(Document.ActiveView, floorPlan.AreaPlan, copingAreas);
        }

        public string UpdateAreaName(Area area, FloorPlan floorPlan) {
            var roomName = area.GetParamValue<string>(BuiltInParameter.ROOM_NAME);
            var areaFloorData = new FloorData(roomName);
            var levelFloorData = GetLevelFloorData(floorPlan, areaFloorData);

            var newAreaName = $"{floorPlan.AreaPlan.Name}" +
                              $"_{areaFloorData.BlockTypeName}" +
                              $"_{levelFloorData?.Elevation ?? "нет уровня"}";
            area.SetParamValue(BuiltInParameter.ROOM_NAME, newAreaName);

            return newAreaName;
        }

        private IEnumerable<Area> CopyAreaToView(View sourceView, View targetView, IEnumerable<Area> copingAreas) {
            ElementId[] copyAreaIds = copingAreas.Select(item => item.Id).ToArray();
            return ElementTransformUtils.CopyElements(sourceView, copyAreaIds,
                    targetView, Transform.Identity, new CopyPasteOptions())
                .Select(item => Document.GetElement(item))
                .OfType<Area>();
        }

        private FloorData GetLevelFloorData(FloorPlan floorPlan, FloorData areaFloorData) {
            string levelName = $"{floorPlan.AreaPlan.Name}_{areaFloorData.BlockTypeName}";
            var level = floorPlan.Levels.FirstOrDefault(item => item.Name.StartsWith(levelName));
            return level == null ? null : new FloorData(level.Name);
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