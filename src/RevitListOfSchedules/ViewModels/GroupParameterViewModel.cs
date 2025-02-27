using Autodesk.Revit.DB;

namespace RevitListOfSchedules.ViewModels {
    public class GroupParameterViewModel {

        public GroupParameterViewModel(Parameter parameter = null) {

            Parameter = parameter;
            Name = GetParamName();
        }

        public string Name { get; }
        public Parameter Parameter { get; }

        public string GetParamName() {
            if(Parameter != null) {
                return Parameter.Definition.Name;
            } else {
                return "< Нет параметра группировки >";
            }
        }
    }
}
