using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.ViewModels.Validation;

namespace RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
internal abstract class PluginParamVM : ValidatableVM {
    private string _paramName;
    private string _paramComment;
    //private bool _errorInParamName;
    //private bool _errorInParamValue;

    protected PluginParamVM(
        SheetSetParametersListVM sheetSetParamsList,
        string paramName,
        string paramComment,
        ILocalizationService localizationService)
        : base(localizationService) {

        SheetSetParamsList = sheetSetParamsList;
        ParamName = paramName ?? string.Empty;
        ParamComment = paramComment ?? string.Empty;

        ParamNameChangeCommand = RelayCommand.Create<PluginParamVM>(ParamNameChange);
        ParamValueChangeCommand = RelayCommand.Create<PluginParamVM>(ParamValueChange);

        //ValidateParamName();
    }

    public ICommand ParamNameChangeCommand { get; }
    public ICommand ParamValueChangeCommand { get; }

    public SheetSetParametersListVM SheetSetParamsList { get; }


    [Required(ErrorMessage = "Validation.ParamNameIsEmpty")]
    public string ParamName {
        get => _paramName;
        set => RaiseAndSetIfChanged(ref _paramName, value);
    }

    public string ParamComment {
        get => _paramComment;
        set => RaiseAndSetIfChanged(ref _paramComment, value);
    }

    //public bool ErrorInParamName {
    //    get => _errorInParamName;
    //    set => RaiseAndSetIfChanged(ref _errorInParamName, value);
    //}

    //public bool ErrorInParamValue {
    //    get => _errorInParamValue;
    //    set => RaiseAndSetIfChanged(ref _errorInParamValue, value);
    //}

    private void ParamNameChange(PluginParamVM pluginParam) {
        //ValidateParamName();
        //SheetSetParamsList.ValidateParams();
        //if(!ErrorInParamName) {
        //    SheetSetParamsList.SheetSet.UpdateDueParamNameChange(pluginParam);
        //}
        if(!HasErrors) {
            SheetSetParamsList.SheetSet.UpdateDueParamNameChange(pluginParam);
        }
    }

    private void ParamValueChange(PluginParamVM pluginParam) {
        //ValidateParamValue();
        //SheetSetParamsList.ValidateParams();
        SheetSetParamsList.SheetSet.UpdateDueParamValueChange(pluginParam);
    }


    //public void ValidateParamName() {
    //    ErrorInParamName = string.IsNullOrEmpty(ParamName);
    //}

    //public abstract void ValidateParamValue();
}
