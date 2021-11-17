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

        private readonly ElementFilter _filter;

        public RevitRepository(Application application, Document document) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);

            _filter = new ElementMulticategoryFilter(new[] { BuiltInCategory.OST_Rooms });
        }

        public string DocumentName => _document.Title;

        public Element GetElement(ElementId elementId) {
            return _document.GetElement(elementId);
        }

        public IList<SpatialElement> GetSelectedSpatialElements() {
            return _uiDocument.GetSelectedElements()
                .Where(item => _filter.PassesFilter(_document, item.Id))
                .OfType<SpatialElement>()
                .ToList();
        }

        public IList<SpatialElement> GetSpatialElements() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .WherePasses(_filter)
                .OfType<SpatialElement>()
                .ToList();
        }

        public IList<SpatialElement> GetRoomsOnActiveView() {
            return GetSpatialElementsOnView(_document.ActiveView);
        }

        public IList<SpatialElement> GetSpatialElementsOnView(View view) {
            return new FilteredElementCollector(_document, view.Id)
                .WhereElementIsNotElementType()
                .WherePasses(_filter)
                .OfType<SpatialElement>()
                .ToList();
        }

        public IList<SpatialElement> GetRooms(IEnumerable<Phase> phases) {
            var phaseIds = phases.Select(item => item.Id);
            return GetSpatialElements().Where(item => phaseIds.Contains(GetPhaseId(item))).ToList();
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

        public IList<FamilyInstance> GetDoors() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Doors)
                .OfType<FamilyInstance>()
                .ToList();
        }

        /// <summary>
        /// Удаляет все не размещенные помещения.
        /// </summary>
        /// <remarks>Создает свою транзакцию.</remarks>
        public void RemoveUnplacedSpatialElements() {
            var unplacedRooms = GetSpatialElements().Union(GetAllAreas()).Where(item => item.Location == null);
            using(var transaction = new Transaction(_document)) {
                transaction.Start("Удаление не размещенных помещений и зон");

                _document.Delete(unplacedRooms.Select(item => item.Id).ToArray());

                transaction.Commit();
            }
        }

        public IList<Area> GetAllAreas() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Areas)
                .OfType<Area>()
                .ToList();
        }

        public ElementId GetPhaseId(Element element) {
            return (ElementId) element.GetParamValueOrDefault(BuiltInParameter.ROOM_PHASE_ID) ?? ElementId.InvalidElementId;
        }

        public void ShowElement(Element element) {
            SelectElement(element);
            _uiDocument.ShowElements(element);
        }

        public void SelectElement(Element element) {
            _uiDocument.SetSelectedElements(element);
        }

        public Transaction StartTransaction(string transactionName) {
            var transaction = new Transaction(_document);
            transaction.BIMStart(transactionName);

            return transaction;
        }
    }
}