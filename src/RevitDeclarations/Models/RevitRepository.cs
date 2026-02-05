using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models;
internal class RevitRepository {
    private readonly List<ElementOnPhaseStatus> _statuses = [
            ElementOnPhaseStatus.Existing,
            ElementOnPhaseStatus.Demolished,
            ElementOnPhaseStatus.New
        ];

    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    public IList<RevitLinkInstance> GetLinks() {
        var loadedLinkTypes = new FilteredElementCollector(Document)
            .OfClass(typeof(RevitLinkType))
            .Cast<RevitLinkType>()
            .Where(x => !x.IsNestedLink)
            .Where(x => x.GetLinkedFileStatus() == LinkedFileStatus.Loaded);

        var temp = loadedLinkTypes.ToList();

        var filter = new ElementClassFilter(typeof(RevitLinkInstance));

        return loadedLinkTypes
            .SelectMany(x => x.GetDependentElements(filter))
            .Select(Document.GetElement)
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

        var phaseRule = new FilterElementIdRule(
            phaseProvider,
            new FilterNumericEquals(),
            phase.Id);

        var phaseFilter = new ElementParameterFilter(phaseRule);

        return new FilteredElementCollector(document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WherePasses(phaseFilter)
            .OfType<Room>()
            .Select(x => new RoomElement(x, settings))
            .ToList();
    }

    public IReadOnlyCollection<FamilyInstance> GetDoorsOnPhase(Document document, Phase phase) {
        var phaseFilter = new ElementPhaseStatusFilter(phase.Id, _statuses);

        return new FilteredElementCollector(document)
            .OfCategory(BuiltInCategory.OST_Doors)
            .WhereElementIsNotElementType()
            .WherePasses(phaseFilter)
            .OfType<FamilyInstance>()
            .ToList();
    }

    public IReadOnlyCollection<CurveElement> GetRoomSeparationLinesOnPhase(Document document, Phase phase) {
        var phaseFilter = new ElementPhaseStatusFilter(phase.Id, _statuses);

        return new FilteredElementCollector(document)
            .WhereElementIsNotElementType()
            .OfCategory(BuiltInCategory.OST_RoomSeparationLines)
            .WherePasses(phaseFilter)
            .OfType<CurveElement>()
            .ToList();
    }

    public IReadOnlyCollection<FamilyInstance> GetBathInstancesOnPhase(Document document, Phase phase) {
        var notDoorsFilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors, true);
        var notWindowsFilter = new ElementCategoryFilter(BuiltInCategory.OST_Windows, true);
        var phaseFilter = new ElementPhaseStatusFilter(phase.Id, _statuses);

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

    public IEnumerable<IEnumerable<RoomElement>> GroupRooms(IEnumerable<RoomElement> rooms,
                                                            DeclarationSettings settings) {
        var multiStoreyAparts = rooms.Where(x => !string.IsNullOrEmpty(x.GetTextParamValue(settings.MultiStoreyParam)))
            .GroupBy(r => new { l = r.GetTextParamValue(settings.MultiStoreyParam), s = r.GetTextParamValue(settings.SectionParam) })
            .Select(g => new List<RoomElement>(g));

        var oneStoreyAparts = rooms.Where(x => string.IsNullOrEmpty(x.GetTextParamValue(settings.MultiStoreyParam)))
            .GroupBy(r => new {
                r.RoomLevel,
                s = r.GetTextParamValue(settings.GroupingBySectionParam),
                g = r.GetTextParamValue(settings.GroupingByGroupParam)
            })
            .Select(g => new List<RoomElement>(g));

        return oneStoreyAparts
            .Concat(multiStoreyAparts);
    }

    public IReadOnlyList<Phase> GetPhases() {
        return Document
            .Phases
            .OfType<Phase>()
            .ToList();
    }

    public IReadOnlyCollection<Parameter> GetRoomsParamsByStorageType(RevitDocumentViewModel document,
                                                                      StorageType storageType) {
        var room = document?.Room;

        return room == null
            ? []
            : (IReadOnlyCollection<Parameter>) room
            .GetOrderedParameters()
            .Where(x => x.StorageType == storageType)
            .ToList();
    }

    public IReadOnlyCollection<Parameter> GetRoomsParamsByDataType(RevitDocumentViewModel document,
                                                                   ForgeTypeId dataType) {
        var room = document?.Room;

        return room == null
            ? []
            : (IReadOnlyCollection<Parameter>) room
            .GetOrderedParameters()
            .Where(x => x.Definition.GetDataType() == dataType)
            .ToList();
    }
}
