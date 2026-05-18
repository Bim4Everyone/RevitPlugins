using dosymep.WPF.ViewModels;

namespace RevitPackageDocumentation.ViewModels.Parameters;
internal abstract class PluginParamVM : BaseViewModel {
    private string _paramName;
    private string _paramComment;

    public string ParamName {
        get => _paramName;
        set => RaiseAndSetIfChanged(ref _paramName, value);
    }

    public string ParamComment {
        get => _paramComment;
        set => RaiseAndSetIfChanged(ref _paramComment, value);
    }
}
