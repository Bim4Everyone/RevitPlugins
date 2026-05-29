using RevitPackageDocumentation.ViewModels.Configuration;

namespace RevitPackageDocumentation.ViewModels.Parameters;
internal class StringParamVM : PluginParamVM {
    private string _stringValue;

    public StringParamVM(SheetSetVM sheetSetVM, string paramName, string paramComment, string stringValue)
        : base(sheetSetVM, paramName, paramComment) {
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
