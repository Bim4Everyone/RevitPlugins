using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RevitCopingZones.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public AreaScheme GetAreaScheme() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(AreaScheme))
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

        public IEnumerable<Area> GetSelectedAreas() {
            return new UIDocument(Document).Selection
                .PickElementsByRectangle(new AreaFilter(), "Выберите зоны на текущем виде")
                .OfType<Area>();
        }

        public IEnumerable<Area> CopyAreaToView(View targetView, IEnumerable<Area> copingAreas) {
            return CopyAreaToView(Document.ActiveView, targetView, copingAreas);
        }

        public IEnumerable<Area> CopyAreaToView(View sourceView, View targetView, IEnumerable<Area> copingAreas) {
            ElementId[] copyAreaIds = copingAreas.Select(item => item.Id).ToArray();
            return ElementTransformUtils.CopyElements(sourceView, copyAreaIds,
                    targetView, Transform.Identity, new CopyPasteOptions())
                .Select(item => Document.GetElement(item))
                .OfType<Area>();
        }
    }

    internal class AreaFilter : ISelectionFilter {
        private readonly ElementId _areaCatId = new ElementId(BuiltInParameter.ROOM_AREA);

        public bool AllowElement(Element elem) {
            return elem.Category.Id == _areaCatId;
        }

        public bool AllowReference(Reference reference, XYZ position) {
            return false;
        }
    }
}