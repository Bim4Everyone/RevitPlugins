using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitFinishing.Models.Finishing;
using RevitFinishing.ViewModels;

namespace RevitFinishing.Models;

internal class RevitRepository {
    private readonly ICollection<ElementOnPhaseStatus> _phaseStatuses;

    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;

        _phaseStatuses = new Collection<ElementOnPhaseStatus>() {
                ElementOnPhaseStatus.Existing,
                ElementOnPhaseStatus.Demolished,
                ElementOnPhaseStatus.New,
                ElementOnPhaseStatus.Temporary
            };
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    public List<Phase> GetPhases() {
        return Document
            .Phases
            .OfType<Phase>()
            .ToList();
    }

    public ObservableCollection<RoomGroupViewModel> GetRoomsOnPhase(Phase phase) {
        ParameterValueProvider valueProvider = new ParameterValueProvider(new ElementId(BuiltInParameter.ROOM_PHASE));
        FilterNumericEquals ruleEvaluator = new FilterNumericEquals();
        FilterElementIdRule filterRule = new FilterElementIdRule(valueProvider, ruleEvaluator, phase.Id);
        ElementParameterFilter parameterFilter = new ElementParameterFilter(filterRule);

        var rooms = new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WherePasses(parameterFilter)
            .OfType<Room>()
            .GroupBy(x => x.GetParamValueOrDefault(BuiltInParameter.ROOM_NAME, "<Без имени>"))
            .Select(x => new RoomGroupViewModel(x.Key.ToString(), x))
            .OrderBy(x => x.Name)
            .ToList();

        return new ObservableCollection<RoomGroupViewModel>(rooms);
    }

    public IReadOnlyCollection<Element> GetFinishingElementsOnPhase(FinishingCategory finishingCategory, Phase phase) {
        ElementPhaseStatusFilter phaseFilter = new ElementPhaseStatusFilter(phase.Id, _phaseStatuses);
        ElementId parameterId = new ElementId(BuiltInParameter.ELEM_TYPE_PARAM);
        ParameterValueProvider valueProvider = new ParameterValueProvider(parameterId);
        FilterStringContains ruleEvaluator = new FilterStringContains();
#if REVIT_2021_OR_LESS
        FilterStringRule rule = new FilterStringRule(valueProvider, ruleEvaluator, finishingCategory.KeyWord, false);
#else
        FilterStringRule rule = new FilterStringRule(valueProvider, ruleEvaluator, finishingCategory.KeyWord);
#endif
        ElementParameterFilter parameterFilter = new ElementParameterFilter(rule);

        return new FilteredElementCollector(Document)
            .OfCategory(finishingCategory.Category)
            .WhereElementIsNotElementType()
            .WherePasses(parameterFilter)
            .WherePasses(phaseFilter)
            .ToElements()
            .ToList();
    }

    public void SetAll(IList<RoomGroupViewModel> allLevels, bool value) {
        foreach(var level in allLevels) {
            level.IsChecked = value;
        }
    }

    public void InvertAll(IList<RoomGroupViewModel> allLevels) {
        foreach(var level in allLevels) {
            level.IsChecked = !level.IsChecked;
        }
    }
}
