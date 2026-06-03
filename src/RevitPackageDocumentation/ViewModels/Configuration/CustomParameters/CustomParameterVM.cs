using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Parameters;

namespace RevitPackageDocumentation.ViewModels.Configuration.CustomParameters;
internal class CustomParameterVM : BaseViewModel {
    private string _paramNameFormula;
    private string _paramName;
    private string _paramValue;

    public CustomParameterVM(CustomParametersListVM customParamsList, StringParamSetService stringParamSetService) {
        CustomParamsList = customParamsList;
        StrParamSetService = stringParamSetService;
        PropUpdateByFormulaCommand = RelayCommand.Create<string>(PropUpdateByFormula);
    }
    public ICommand PropUpdateByFormulaCommand { get; }


    public CustomParametersListVM CustomParamsList { get; }
    public StringParamSetService StrParamSetService { get; }


    public string ParamValueFormula {
        get => _paramNameFormula;
        set => RaiseAndSetIfChanged(ref _paramNameFormula, value);
    }

    public string ParamName {
        get => _paramName;
        set => RaiseAndSetIfChanged(ref _paramName, value);
    }

    public string ParamValue {
        get => _paramValue;
        set => RaiseAndSetIfChanged(ref _paramValue, value);
    }

    private void PropUpdateByFormula(string formulaPropertyName) {
        StrParamSetService.Set(this, formulaPropertyName, CustomParamsList.SheetComponent.Sheet.SheetSet.Params);
    }

    public void UpdateDueParamValueChange(StringParamVM stringParam) {
        StrParamSetService.SetAll(this, CustomParamsList.SheetComponent.Sheet.SheetSet.Params, stringParam);
    }
}
