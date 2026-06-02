using dosymep.WPF.ViewModels;

namespace RevitPackageDocumentation.ViewModels.Configuration.CustomParameters;
internal class CustomParameterVM : BaseViewModel {
    private string _paramName;
    private string _paramValue;

    public CustomParameterVM(CustomParametersListVM customParamsList) {
        CustomParamsList = customParamsList;
    }

    public CustomParametersListVM CustomParamsList { get; }

    public string ParamName {
        get => _paramName;
        set => RaiseAndSetIfChanged(ref _paramName, value);
    }

    public string ParamValue {
        get => _paramValue;
        set => RaiseAndSetIfChanged(ref _paramValue, value);
    }
}
