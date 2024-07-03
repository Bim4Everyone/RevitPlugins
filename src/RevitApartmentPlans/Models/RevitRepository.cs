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
