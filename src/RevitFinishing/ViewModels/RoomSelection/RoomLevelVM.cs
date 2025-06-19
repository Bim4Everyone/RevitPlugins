using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitFinishing.ViewModels;
internal class RoomLevelVM : SelectionElementVM {
    private readonly Element _level;

    public RoomLevelVM(Element level, string name, BuiltInParameter bltnParam, ILocalizationService localizationService)
        : base(name, bltnParam, localizationService) {
            _level = level;
    }

    public Element Level => _level;

    public override ElementParameterFilter GetParameterFilter() {
        var paramId = new ElementId(BuiltInParameter.ROOM_UPPER_LEVEL);
        var valueProvider = new ParameterValueProvider(paramId);
        var ruleEvaluator = new FilterNumericEquals();
        var filterRule = new FilterElementIdRule(valueProvider, ruleEvaluator, Level.Id);
        return new ElementParameterFilter(filterRule);
    }
}
