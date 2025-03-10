using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

namespace RevitListOfSchedules.ViewModels {
    public class GroupParameterViewModel {
        private readonly ILocalizationService _localizationService;
        private readonly RevitParam _parameter;
        private readonly string _name;

        public GroupParameterViewModel(ILocalizationService localizationService, RevitParam parameter = null) {
            _localizationService = localizationService;
            _parameter = parameter;
            _name = GetParamName();
        }

        public RevitParam Parameter => _parameter;
        public string Name => _name;

        public string GetParamName() {
            if(_parameter != null) {
                return _parameter.Name;
            } else {
                return _localizationService.GetLocalizedString("GroupParameter.NoParameter");
            }
        }
    }
}
