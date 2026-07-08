using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.ViewModels.Validation;

namespace RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
internal abstract class PluginParamVM : ValidatableVM {
    private string _paramName;
    private string _paramComment;

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

    private void ParamNameChange(PluginParamVM pluginParam) {
        if(!HasErrors) {
            SheetSetParamsList.SheetSet.UpdateDueParamNameChange(pluginParam);
        }
    }

    private void ParamValueChange(PluginParamVM pluginParam) {
        SheetSetParamsList.SheetSet.UpdateDueParamValueChange(pluginParam);
    }
}
