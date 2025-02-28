using Autodesk.Revit.DB;

namespace RevitListOfSchedules.ViewModels {
    public class GroupParameterViewModel {

        private readonly Parameter _parameter;
        private readonly string _name;

        public GroupParameterViewModel(Parameter parameter = null) {
            _parameter = parameter;
            _name = GetParamName();
        }

        public Parameter Parameter => _parameter;
        public string Name => _name;

        public string GetParamName() {
            if(_parameter != null) {
                return _parameter.Definition.Name;
            } else {
                return "< Нет параметра группировки >";
            }
        }
    }
}
