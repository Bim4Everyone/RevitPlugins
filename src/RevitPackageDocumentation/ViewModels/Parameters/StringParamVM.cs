namespace RevitPackageDocumentation.ViewModels.Parameters;
internal class StringParamVM : PluginParamVM {
    private string _stringValue;

    public string StringValue {
        get => _stringValue;
        set => RaiseAndSetIfChanged(ref _stringValue, value);
    }
}
