using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitFinishing.ViewModels
{
    internal class RoomLevelVM : SelectionElementVM {
        private readonly Element _level;
        public RoomLevelVM(Element level, string name, BuiltInParameter bltnParam, ILocalizationService localizationService) 
            : base(name, bltnParam, localizationService) {
            _level = level;
        }

        public Element Level => _level;

        public ElementParameterFilter GetElementIdFilter() {
            ElementId paramId = new ElementId(BuiltInParameter.ROOM_UPPER_LEVEL);
            ParameterValueProvider valueProvider = new ParameterValueProvider(paramId);
            FilterNumericEquals ruleEvaluator = new FilterNumericEquals();
            FilterElementIdRule filterRule = new FilterElementIdRule(valueProvider, ruleEvaluator, Level.Id);
            return new ElementParameterFilter(filterRule);
        }
    }
}
