using dosymep.SimpleServices;

namespace RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
internal class StringParamVM : PluginParamVM {
    private string _stringValue;

    public StringParamVM(
        SheetSetParametersListVM sheetSetParamsList,
        string paramName,
        string paramComment,
        string stringValue,
        ILocalizationService localizationService)
        : base(sheetSetParamsList, paramName, paramComment, localizationService) {
        StringValue = stringValue ?? string.Empty;
        ValidateAllProperties();
    }

    public string StringValue {
        get => _stringValue;
        set => RaiseAndSetIfChanged(ref _stringValue, value);
    }
}
