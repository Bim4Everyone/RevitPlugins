namespace RevitPackageDocumentation.ViewModels.Parameters;
internal class StringParamVM : PluginParamVM {
    private string _stringValue;

    public StringParamVM(string paramName, string paramComment, string stringValue) : base(paramName, paramComment) {
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
