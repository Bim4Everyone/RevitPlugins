using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters;

namespace RevitPackageDocumentation.ViewModels.Parameters;
internal class StringParamVM : PluginParamVM {
    private string _stringValue;

    public StringParamVM(SheetSetParametersListVM sheetSetParamsList, string paramName, string paramComment, string stringValue)
        : base(sheetSetParamsList, paramName, paramComment) {
        StringValue = stringValue ?? string.Empty;
        ValidateParamValue();
    }

    public string StringValue {
        get => _stringValue;
        set => RaiseAndSetIfChanged(ref _stringValue, value);
    }

    public override void ValidateParamValue() {
        ErrorInParamValue = string.IsNullOrEmpty(StringValue);
    }
}
