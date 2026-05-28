using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitPackageDocumentation.ViewModels.Parameters;
internal abstract class PluginParamVM : BaseViewModel {
    private string _paramName;
    private string _paramComment;
    private bool _errorInParamName;
    private bool _errorInParamValue;

    protected PluginParamVM(string paramName, string paramComment) {
        ParamName = paramName ?? string.Empty;
        ParamComment = paramComment ?? string.Empty;

        ValidateParamNameCommand = RelayCommand.Create(ValidateParamName);
        ValidateParamValueCommand = RelayCommand.Create(ValidateParamValue);

        ValidateParamName();
    }

    public ICommand ValidateParamNameCommand { get; }
    public ICommand ValidateParamValueCommand { get; }

    public string ParamName {
        get => _paramName;
        set => RaiseAndSetIfChanged(ref _paramName, value);
    }

    public string ParamComment {
        get => _paramComment;
        set => RaiseAndSetIfChanged(ref _paramComment, value);
    }

    public bool ErrorInParamName {
        get => _errorInParamName;
        set => RaiseAndSetIfChanged(ref _errorInParamName, value);
    }

    public bool ErrorInParamValue {
        get => _errorInParamValue;
        set => RaiseAndSetIfChanged(ref _errorInParamValue, value);
    }

    public void ValidateParamName() {
        ErrorInParamName = string.IsNullOrEmpty(ParamName);
    }

    public abstract void ValidateParamValue();
}
