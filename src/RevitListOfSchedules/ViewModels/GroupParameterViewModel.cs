using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

namespace RevitListOfSchedules.ViewModels;
public class GroupParameterViewModel {
    private readonly ILocalizationService _localizationService;

    public GroupParameterViewModel(ILocalizationService localizationService, RevitParam parameter = null) {
        _localizationService = localizationService;
        Parameter = parameter;
        Name = GetParamName();
        Id = GetId();
    }

    public RevitParam Parameter { get; }
    public string Name { get; }
    public string Id { get; }

    private string GetParamName() {
        return Parameter != null ? Parameter.Name : _localizationService.GetLocalizedString("GroupParameter.NoParameter");
    }

    private string GetId() {
        return Parameter != null ? Parameter.Id : string.Empty;
    }
}
