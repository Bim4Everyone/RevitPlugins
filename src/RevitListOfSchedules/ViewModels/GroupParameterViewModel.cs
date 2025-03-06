using dosymep.Bim4Everyone;

namespace RevitListOfSchedules.ViewModels {
    public class GroupParameterViewModel {

        private readonly RevitParam _parameter;
        private readonly string _name;

        public GroupParameterViewModel(RevitParam parameter = null) {
            _parameter = parameter;
            _name = GetParamName();
        }

        public RevitParam Parameter => _parameter;
        public string Name => _name;

        public string GetParamName() {
            if(_parameter != null) {
                return _parameter.Name;
            } else {
                return "< Нет параметра группировки >";
            }
        }
    }
}
