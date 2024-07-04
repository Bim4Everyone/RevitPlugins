using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;

using RevitApartmentPlans.Services;

namespace RevitApartmentPlans.Models {
    internal class RevitRepository {
        private readonly SpatialElementBoundaryOptions _spatialElementBoundaryOptions;


        public RevitRepository(UIApplication uiApplication) {
            _spatialElementBoundaryOptions = new SpatialElementBoundaryOptions() {
                SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish,
                StoreFreeBoundaryFaces = false
            };
            UIApplication = uiApplication;
        }


        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;


        /// <summary>
        /// Возвращает все замкнутые контуры границ помещения
        /// </summary>
        /// <param name="room">Помещение</param>
        /// <returns>Список всех замкнутых контуров границ помещения</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IList<CurveLoop> GetBoundaryCurveLoops(Room room) {
            if(room is null) { throw new ArgumentNullException(nameof(room)); }

            return room.GetBoundarySegments(_spatialElementBoundaryOptions)
                .Select(loop => CurveLoop.Create(loop.Select(c => c.GetCurve()).ToArray()))
                .ToArray();
        }

        /// <summary>
        /// TODO debug only method
        /// </summary>
        /// <returns></returns>
        public Apartment GetDebugApartment() {
            return new Apartment(PickRooms(), "test");
        }

        /// <summary>
        /// TODO debug only method
        /// </summary>
        /// <returns></returns>
        public ICollection<ViewPlan> GetDebugTemplates() {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(ViewPlan))
                .Cast<ViewPlan>()
                .Where(plan => plan.IsTemplate
                    && (plan.ViewType == ViewType.FloorPlan || plan.ViewType == ViewType.CeilingPlan))
                .ToArray();
        }

        /// <summary>
        /// TODO debug only method
        /// </summary>
        /// <param name="curveLoop"></param>
        public void CreateDebugLines(CurveLoop curveLoop) {
            using(Transaction t = Document.StartTransaction("Создание тестового контура")) {
                Plane geomPlane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, curveLoop.First().GetEndPoint(0));
                SketchPlane sketch = SketchPlane.Create(Document, geomPlane);

                foreach(var item in curveLoop) {
                    Document.Create.NewModelCurve(item, sketch);
                }

                t.Commit();
            }
        }

        /// <summary>
        /// Возвращает тип шаблона вида: план этажа/план потолка
        /// </summary>
        /// <param name="template">Шаблон вида</param>
        /// <exception cref="NotSupportedException">Исключение, если шаблон вида - не план этажа/потолка</exception>
        public ElementId GetViewFamilyTypeId(ViewPlan template) {
            switch(template.ViewType) {
                case ViewType.FloorPlan:
                    return GetViewFamilyTypeId(ViewFamily.FloorPlan);
                case ViewType.CeilingPlan:
                    return GetViewFamilyTypeId(ViewFamily.CeilingPlan);
                default:
                    throw new NotSupportedException($"Тип шаблона {template.ViewType} не поддерживается");
            }
        }

        private ElementId GetViewFamilyTypeId(ViewFamily viewFamily) {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .Where(v => v.ViewFamily == viewFamily)
                .First()
                .Id;
        }

        /// <summary>
        /// TODO debug only method
        /// </summary>
        /// <returns></returns>
        private ICollection<Room> PickRooms() {
            ISelectionFilter filter = new SelectionFilterRooms(Document);
            IList<Reference> references = ActiveUIDocument.Selection.PickObjects(
                ObjectType.Element,
                filter,
                "Выберите помещения");

            List<Room> rooms = new List<Room>();
            foreach(var reference in references) {
                if((reference != null) && (Document.GetElement(reference) is Room room) && (room.Area > 0)) {
                    rooms.Add(room);
                }
            }
            return rooms;
        }
    }
}
