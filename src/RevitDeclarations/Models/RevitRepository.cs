using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public IList<RevitLinkInstance> GetLinks() {
            IEnumerable<RevitLinkType> loadedLinkTypes = new FilteredElementCollector(Document)
                .OfClass(typeof(RevitLinkType))
                .Cast<RevitLinkType>()
                .Where(x => x.GetLinkedFileStatus() == LinkedFileStatus.Loaded);

            ElementClassFilter filter = new ElementClassFilter(typeof(RevitLinkInstance));

            return loadedLinkTypes
                .SelectMany(x => x.GetDependentElements(filter))
                .Select(x => Document.GetElement(x))
                .Cast<RevitLinkInstance>()
                .ToList();
        }

        public Phase GetPhaseByName(Document document, string phaseName) {
            return document
                .Phases
                .OfType<Phase>()
                .Where(x => x.Name == phaseName)
                .FirstOrDefault();
        }

        public IReadOnlyCollection<RoomElement> GetRoomsOnPhase(Document document, 
                                                                Phase phase, 
                                                                DeclarationSettings settings) {
            var phaseProvider = new ParameterValueProvider(new ElementId(BuiltInParameter.ROOM_PHASE));

            FilterElementIdRule phaseRule = new FilterElementIdRule(
                phaseProvider,
                new FilterNumericEquals(),
                phase.Id);

            ElementParameterFilter phaseFilter = new ElementParameterFilter(phaseRule);

            return new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WherePasses(phaseFilter)
                .OfType<Room>()
                .Select(x => new RoomElement(x, settings))
                .ToList();
        }

        public IReadOnlyCollection<FamilyInstance> GetDoorsOnPhase(Document document, Phase phase) {
            List<ElementOnPhaseStatus> statuses = new List<ElementOnPhaseStatus>() {
                ElementOnPhaseStatus.Existing,
                ElementOnPhaseStatus.Demolished,
                ElementOnPhaseStatus.New,
                ElementOnPhaseStatus.Temporary
            };

            ElementPhaseStatusFilter phaseFilter = new ElementPhaseStatusFilter(phase.Id, statuses);

            return new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .WherePasses(phaseFilter)
                .OfType<FamilyInstance>()
                .ToList();
        }

        public IReadOnlyCollection<FamilyInstance> GetBathInstancesOnPhase(Document document, Phase phase) {
            ElementCategoryFilter notDoorsFilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors, true);
            ElementCategoryFilter notWindowsFilter = new ElementCategoryFilter(BuiltInCategory.OST_Windows, true);

            List<ElementOnPhaseStatus> statuses = new List<ElementOnPhaseStatus>() {
                ElementOnPhaseStatus.Existing,
                ElementOnPhaseStatus.Demolished,
                ElementOnPhaseStatus.New,
                ElementOnPhaseStatus.Temporary
            };

            ElementPhaseStatusFilter phaseFilter = new ElementPhaseStatusFilter(phase.Id, statuses);

            /// Поиск семейств ванн и душевых кабин по наличию "ванна" или "душев" в имени семейства.
            /// Также исключается семейства с суффиксом "ова", например, заканчиваюищеся на "ованная"
            return new FilteredElementCollector(document)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .WherePasses(notDoorsFilter)
                .WherePasses(notWindowsFilter)
                .WherePasses(phaseFilter)
                .OfType<FamilyInstance>()
                .Where(x => x.Symbol.Family.Name.ToLower().Contains("ванна") || 
                    x.Symbol.Family.Name.ToLower().Contains("душев"))
                .Where(x => !x.Symbol.Family.Name.ToLower().Contains("ованна"))
                .ToList();
        }

        public IReadOnlyCollection<Apartment> GetApartments(IEnumerable<RoomElement> rooms, 
                                                            DeclarationSettings settings) {
            var multiStoreyAparts = rooms.Where(x => !string.IsNullOrEmpty(x.GetTextParamValue(settings.MultiStoreyParam)))
                .GroupBy(r => new { l = r.GetTextParamValue(settings.MultiStoreyParam), s = r.GetTextParamValue(settings.SectionParam) })
                .Select(g => new Apartment(g, settings))
                .ToList();

            var oneStoreyAparts = rooms.Where(x => string.IsNullOrEmpty(x.GetTextParamValue(settings.MultiStoreyParam)))
                .GroupBy(r => new { r.RoomLevel, s = r.GetTextParamValue(settings.SectionParam), g = r.GetTextParamValue(settings.GroupingByGroupParam) })
                .Select(g => new Apartment(g, settings))
                .ToList();            

            return oneStoreyAparts
                .Concat(multiStoreyAparts)
                .ToList();
        }

        public IReadOnlyList<Phase> GetPhases() {
            return Document
                .Phases
                .OfType<Phase>()
                .ToList();
        }

        public IReadOnlyCollection<Parameter> GetRoomsParamsByStorageType(RevitDocumentViewModel document,
                                                                          StorageType storageType) {
            Room room = document?.Room;

            if(room == null) {
                return new List<Parameter>();
            }

            return room
                .GetOrderedParameters()
                .Where(x => x.StorageType == storageType)
                .ToList();
        }
    }
}
