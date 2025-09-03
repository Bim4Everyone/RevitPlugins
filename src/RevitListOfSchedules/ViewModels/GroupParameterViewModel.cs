using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

namespace RevitListOfSchedules.ViewModels;
public class GroupParameterViewModel {
    private readonly ILocalizationService _localizationService;

    public GroupParameterViewModel(ILocalizationService localizationService, RevitParam parameter = null) {
        _localizationService = localizationService;
        Parameter = parameter;
    }

    public RevitParam Parameter { get; }
    public string Name => GetParamName();
    public string Id => GetId();

    private string GetParamName() {
        return Parameter != null ? Parameter.Name : _localizationService.GetLocalizedString("GroupParameter.NoParameter");
    }

    private string GetId() {
        return Parameter != null ? Parameter.Id : string.Empty;
    }
}
