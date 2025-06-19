using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

namespace RevitListOfSchedules.ViewModels {
    public class GroupParameterViewModel {
        private readonly ILocalizationService _localizationService;
        private readonly RevitParam _parameter;
        private readonly string _name;
        private readonly string _id;

        public GroupParameterViewModel(ILocalizationService localizationService, RevitParam parameter = null) {
            _localizationService = localizationService;
            _parameter = parameter;
            _name = GetParamName();
            _id = GetId();
        }

        public RevitParam Parameter => _parameter;
        public string Name => _name;
        public string Id => _id;

        private string GetParamName() {
            if(_parameter != null) {
                return _parameter.Name;
            } else {
                return _localizationService.GetLocalizedString("GroupParameter.NoParameter");
            }
        }

        private string GetId() {
            if(_parameter != null) {
                return _parameter.Id;
            }
            return string.Empty;
        }
    }
}
