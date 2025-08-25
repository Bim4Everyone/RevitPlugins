using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.KeySchedules;
using dosymep.Revit;

using RevitFinishing.Models.Finishing;

namespace RevitFinishing.Models;

internal class RevitRepository {
    private readonly ICollection<ElementOnPhaseStatus> _phaseStatuses;

    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;

        _phaseStatuses = [
                ElementOnPhaseStatus.Existing,
                ElementOnPhaseStatus.Demolished,
                ElementOnPhaseStatus.New,
                ElementOnPhaseStatus.Temporary
            ];
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    public IList<Phase> GetPhases() {
        return Document
            .Phases
            .OfType<Phase>()
            .ToList();
    }

    public IList<string> GetParamStringValues(Phase phase, BuiltInParameter param) {
        var valueProvider = new ParameterValueProvider(new ElementId(BuiltInParameter.ROOM_PHASE));
        var ruleEvaluator = new FilterNumericEquals();
        var filterRule = new FilterElementIdRule(valueProvider, ruleEvaluator, phase.Id);
        var parameterFilter = new ElementParameterFilter(filterRule);

        return new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WherePasses(parameterFilter)
            .OfType<Room>()
            .Select(x => x.GetParamValueOrDefault<string>(param))
            .Distinct()
            .ToList();
    }

    public IEnumerable<Element> GetRoomLevels(Phase phase) {
        var valueProvider = new ParameterValueProvider(new ElementId(BuiltInParameter.ROOM_PHASE));
        var ruleEvaluator = new FilterNumericEquals();
        var filterRule = new FilterElementIdRule(valueProvider, ruleEvaluator, phase.Id);
        var parameterFilter = new ElementParameterFilter(filterRule);

        return new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WherePasses(parameterFilter)
            .OfType<Room>()
            .Select(x => x.Level)
            .Where(x => x != null)
            .GroupBy(x => new { x.Id, x.Name })
            .Select(g => g.First());
    }

    public IReadOnlyCollection<FinishingElement> GetFinishingElementsOnPhase(
        FinishingCategory finishingCategory,
        IFinishingFactory factory,
        Phase phase) {
        var phaseFilter = new ElementPhaseStatusFilter(phase.Id, _phaseStatuses);
        var parameterId = new ElementId(BuiltInParameter.ELEM_TYPE_PARAM);
        var valueProvider = new ParameterValueProvider(parameterId);
        var ruleEvaluator = new FilterStringContains();
#if REVIT_2021_OR_LESS
        FilterStringRule rule = new FilterStringRule(valueProvider, ruleEvaluator, finishingCategory.KeyWord, false);
#else
        var rule = new FilterStringRule(valueProvider, ruleEvaluator, finishingCategory.KeyWord);
#endif
        var parameterFilter = new ElementParameterFilter(rule);

        var categoryFilter = new ElementMulticategoryFilter(finishingCategory.Category);

        return new FilteredElementCollector(Document)
            .WherePasses(categoryFilter)
            .WhereElementIsNotElementType()
            .WherePasses(parameterFilter)
            .WherePasses(phaseFilter)
            .ToElements()
            .Select(x => factory.Create(x))
            .ToList();
    }

    public IList<Room> GetRoomsByFilters(IList<ElementFilter> orFilters) {
        var finalFilter = new LogicalAndFilter(orFilters);

        return new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WherePasses(finalFilter)
            .OfType<Room>()
            .ToList();
    }

    public IList<KeyFinishingType> GetKeyFinishingTypes() {
        Element schedule = new FilteredElementCollector(Document)
            .OfClass(typeof(ViewSchedule))
            .Where(x => x.Name == KeySchedulesConfig.Instance.RoomsFinishing.ScheduleName)
            .First();

       return new FilteredElementCollector(Document, schedule.Id)
            .ToElements()
            .Select(x => new KeyFinishingType(x))
            .ToList();
    }
}
