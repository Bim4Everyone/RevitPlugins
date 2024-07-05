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
        /// Возвращает массив используемых типов видов
        /// </summary>
        public ViewType[] GetAllUsedViewTypes() {
            return new ViewType[] {
                ViewType.FloorPlan,
                ViewType.CeilingPlan
            };
        }

        /// <summary>
        /// По заданным Id находит все шаблоны видов из активного документа, по которым можно создать планы
        /// </summary>
        /// <param name="viewPlanIds">Id шаблонов видов</param>
        public ICollection<ViewPlan> GetViewPlans(ICollection<ElementId> viewPlanIds) {
            var enabledViewTypes = GetAllUsedViewTypes();
            return new FilteredElementCollector(Document, viewPlanIds)
                .OfClass(typeof(ViewPlan))
                .Cast<ViewPlan>()
                .Where(plan => plan.IsTemplate && enabledViewTypes.Any(vt => vt == plan.ViewType))
                .ToArray();
        }

        public ICollection<Apartment> GetApartments(string paramName) {
            if(string.IsNullOrWhiteSpace(paramName)) { throw new ArgumentException(nameof(paramName)); }

            return new FilteredElementCollector(Document, Document.ActiveView.Id)
                .WherePasses(new RoomFilter())
                .Cast<Room>()
                .Where(r => r.Area > 0 && !string.IsNullOrWhiteSpace(r.GetParamValue<string>(paramName)))
                .GroupBy(r => r.GetParamValue<string>(paramName))
                .Select(g => new Apartment(g.ToArray(), g.Key))
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
            var enabledViewTypes = GetAllUsedViewTypes();
            return new FilteredElementCollector(Document)
                .OfClass(typeof(ViewPlan))
                .Cast<ViewPlan>()
                .Where(plan => plan.IsTemplate && enabledViewTypes.Any(vt => vt == plan.ViewType))
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


        /// <summary>
        /// Возвращает уровень, привязанный к активному виду, который должен быть планом
        /// </summary>
        /// <exception cref="InvalidOperationException">Исключение, если активный вид - не план</exception>
        private Level GetLevelOfActivePlan() {
            var view = Document.ActiveView as ViewPlan;
            if(view is null) {
                throw new InvalidOperationException("Активный вид не является планом");
            }
            return view.GenLevel;
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