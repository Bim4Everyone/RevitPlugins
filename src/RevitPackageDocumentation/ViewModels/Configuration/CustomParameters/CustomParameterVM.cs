using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.Validation;

namespace RevitPackageDocumentation.ViewModels.Configuration.CustomParameters;
internal class CustomParameterVM : ValidatableVM {
    private string _paramValueFormula = string.Empty;
    private string _paramName;
    private string _paramValue = string.Empty;

    public CustomParameterVM(CustomParametersListVM customParamsList, StringParamSetService stringParamSetService,
        ILocalizationService localizationService)
        : base(localizationService) {

        CustomParamsList = customParamsList;
        StrParamSetService = stringParamSetService;
        ValidateAllProperties();

        PropUpdateByFormulaCommand = RelayCommand.Create<string>(PropUpdateByFormula);
    }

    public ICommand PropUpdateByFormulaCommand { get; }

    public CustomParametersListVM CustomParamsList { get; }
    public StringParamSetService StrParamSetService { get; }

    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\];~]+$")]
    public string ParamName {
        get => _paramName;
        set => RaiseAndSetIfChanged(ref _paramName, value);
    }

    public string ParamValueFormula {
        get => _paramValueFormula;
        set => RaiseAndSetIfChanged(ref _paramValueFormula, value);
    }

    public string ParamValue {
        get => _paramValue;
        set => RaiseAndSetIfChanged(ref _paramValue, value);
    }

    private void PropUpdateByFormula(string formulaPropertyName) {
        StrParamSetService.Set(this, formulaPropertyName, CustomParamsList.SheetSetParams);
    }

    /// <summary>
    /// В случае изменения имени параметра конфигурации нужно обновить свойства дополнительного параметра
    /// </summary>
    public void UpdateDueParamNameChange() {
        StrParamSetService.SetAll(this, CustomParamsList.SheetSetParams);
    }

    public void UpdateDueParamValueChange(StringParamVM stringParam) {
        StrParamSetService.SetAll(this, CustomParamsList.SheetSetParams, stringParam);
    }
}
