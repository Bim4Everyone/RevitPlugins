using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using DevExpress.Utils.Extensions;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.KeySchedules;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitRooms.ViewModels;

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
        
        public IList<FamilyInstance> GetWindows() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Windows)
                .OfType<FamilyInstance>()
                .ToList();
        }

        public IEnumerable<CurveElement> GetRoomSeparators() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_RoomSeparationLines)
                .OfType<CurveElement>()
                .ToList();
        }

        public void UpdateLevelSharedParam(SpatialElement spatialElement, Dictionary<ElementId, string> levelNames) {
            spatialElement.SetParamValue(SharedParamsConfig.Instance.Level,
                levelNames.GetValueOrDefault(spatialElement.Level.Id, spatialElement.Level.Name));
        }

        private string GetLevelName(Level level) {
            int index = level.Name.IndexOf(' ');
            return index <= 0
                ? level.Name
                : level.Name.Substring(0, index);
        }

        public Dictionary<ElementId, string> GetLevelNames() {
            var levels = GetSpatialElements()
                .Select(item => item.Level)
                .Distinct(new ElementComparer())
                .Cast<Level>()
                .ToArray();

            (ElementId ElementId, string LevelName)[] levelPairs = levels
                .Select(item => (item.Id, GetLevelName(item)))
                .ToArray();
            
            bool isOneUnderLevel = levelPairs
                .Where(item => item.LevelName.StartsWith("П", StringComparison.CurrentCultureIgnoreCase))
                .GroupBy(item => item.LevelName)
                .Count() == 1;
            
            var levelNames = new Dictionary<ElementId, string>();
            foreach((ElementId elementId, string levelName) in levelPairs) {
                if(levelName.StartsWith("К", StringComparison.CurrentCultureIgnoreCase)) {
                    levelNames.Add(elementId, "Кровля");
                } else if(levelName.StartsWith("Т", StringComparison.CurrentCultureIgnoreCase)) {
                    levelNames.Add(elementId, "Технический");
                } else if(levelName.StartsWith("П", StringComparison.CurrentCultureIgnoreCase)) {
                    if(isOneUnderLevel) {
                        levelNames.Add(elementId, "Подземный");
                    } else if(int.TryParse(levelName.Substring(1), out int index)) {
                        levelNames.Add(elementId, $"Подземный -{index}");
                    }
                } else if(Regex.IsMatch(levelName, "[0-9]+")) {
                    if(int.TryParse(levelName, out int index)) {
                        levelNames.Add(elementId, index.ToString());
                    }
                } else {
                    levelNames.Add(elementId, levelName);
                }
            }

            return levelNames;
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