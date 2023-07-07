using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.KeySchedules;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Revit;

namespace RevitRooms.Models {
    internal class RevitRepository {
        private readonly ElementFilter _filter;

        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
            _filter = new ElementMulticategoryFilter(new[] { BuiltInCategory.OST_Rooms, BuiltInCategory.OST_Areas });
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;
        
        public string DocumentName => Document.Title;

        public Element GetElement(ElementId elementId) {
            return Document.GetElement(elementId);
        }

        public IList<SpatialElement> GetSelectedSpatialElements() {
            return ActiveUIDocument.GetSelectedElements()
                .Where(item => _filter.PassesFilter(Document, item.Id))
                .OfType<SpatialElement>()
                .ToList();
        }

        public IList<SpatialElement> GetSpatialElements() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .WherePasses(_filter)
                .OfType<SpatialElement>()
                .ToList();
        }

        public IList<SpatialElement> GetRoomsOnActiveView() {
            return GetSpatialElementsOnView(Document.ActiveView);
        }

        public IList<SpatialElement> GetSpatialElementsOnView(View view) {
            return new FilteredElementCollector(Document, view.Id)
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
            return (Phase) Document.GetElement(GetPhaseId(element));
        }

        public IList<Phase> GetAdditionalPhases() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Phase))
                .Where(item => item.Name.Equals("Контур здания") || item.Name.Equals("Межквартирные перегородки"))
                .OfType<Phase>()
                .ToList();
        }

        public IList<FamilyInstance> GetDoors() {
            return new FilteredElementCollector(Document)
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
            var unplacedRooms = GetSpatialElements().Union(GetAllAreas()).Where(item => item.Location == null || item.Level == null);
            using(var transaction = new Transaction(Document)) {
                transaction.Start("Удаление не размещенных помещений и зон");

                Document.Delete(unplacedRooms.Select(item => item.Id).ToArray());

                transaction.Commit();
            }
        }

        public IList<Area> GetAllAreas() {
            return new FilteredElementCollector(Document)
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
            ActiveUIDocument.ShowElements(element);
        }

        public void SelectElement(Element element) {
            ActiveUIDocument.SetSelectedElements(element);
        }

        public Transaction StartTransaction(string transactionName) {
            var transaction = new Transaction(Document);
            transaction.BIMStart(transactionName);

            return transaction;
        }

        public IList<Element> GetNumberingOrders() {
            ViewSchedule viewSchedule = (ViewSchedule) new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Schedules)
                .FirstOrDefault(item => item.Name.Equals(KeySchedulesConfig.Instance.RoomsNames.ScheduleName));

            return new FilteredElementCollector(Document, viewSchedule.Id)
                .WhereElementIsNotElementType()
                .ToElements();
        }
    }
}