using dosymep.WPF.ViewModels;

namespace RevitPackageDocumentation.ViewModels.Configuration.AdditionalParameters;
internal class AdditionalParameterVM : BaseViewModel {
    private string _paramName;
    private string _paramValue;

    public AdditionalParameterVM(AdditionalParametersListVM additionalParamsList) {
        AdditionalParamsList = additionalParamsList;
    }

    public AdditionalParametersListVM AdditionalParamsList { get; }

    public string ParamName {
        get => _paramName;
        set => RaiseAndSetIfChanged(ref _paramName, value);
    }

    public string ParamValue {
        get => _paramValue;
        set => RaiseAndSetIfChanged(ref _paramValue, value);
    }
}
