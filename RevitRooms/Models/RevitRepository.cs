using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitRooms.Models {
    internal class RevitRepository {
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;

        public RevitRepository(Application application, Document document) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);
        }

        public IList<Room> GetSelectedRooms() {
            return _uiDocument.GetSelectedElements()
                .OfType<Room>()
                .ToList();
        }

        public IList<Room> GetAllRooms() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Room))
                .OfType<Room>()
                .ToList();
        }

        public IList<Room> GetRoomsOnActiveView() {
            return GetRoomsOnView(_document.ActiveView);
        }

        public IList<Room> GetRoomsOnView(View view) {
            return new FilteredElementCollector(_document, view.Id)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Room))
                .OfType<Room>()
                .ToList();
        }

        public IList<Room> GetRooms(IEnumerable<Phase> phases) {
            var phaseIds = phases.Select(item => item.Id);
            return GetAllRooms().Where(item => phaseIds.Contains(GetPhaseId(item))).ToList();
        }

        public Phase GetPhase(Element element) {
            return (Phase) _document.GetElement(GetPhaseId(element));
        }

        public IList<Phase> GetAdditionalPhases() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Phase))
                .Where(item => item.Name.Equals("Контур здания") || item.Name.Equals("Межквартирные перегородки"))
                .OfType<Phase>()
                .ToList();
        }

        /// <summary>
        /// Удаляет все не размещенные помещения.
        /// </summary>
        /// <remarks>Создает свою транзакцию.</remarks>
        public void RemoveUnplacedRooms() {
            var unplacedRooms = GetAllRooms().Where(item => item.Location == null);
            using(var transaction = new Transaction(_document)) {
                transaction.Start("Удаление не размещенных помещений");

                _document.Delete(unplacedRooms.Select(item => item.Id).ToArray());

                transaction.Commit();
            }
        }

        public ElementId GetPhaseId(Element element) {
            return (ElementId) element.GetParamValueOrDefault(BuiltInParameter.ROOM_PHASE_ID);
        }
    }
}
