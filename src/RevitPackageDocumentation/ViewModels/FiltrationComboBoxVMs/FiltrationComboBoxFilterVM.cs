using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;

namespace RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;
internal class FiltrationComboBoxFilterVM : BaseViewModel {
    private string _valueFormula = string.Empty;
    private string _value = string.Empty;

    public FiltrationComboBoxFilterVM(FiltrationComboBoxFilterListVM filterList, StringParamSetService stringParamSetService) {
        FilterList = filterList;
        StrParamSetService = stringParamSetService;
        PropUpdateByFormulaCommand = RelayCommand.Create<string>(PropUpdateByFormula);
    }

    public ICommand PropUpdateByFormulaCommand { get; }

    public FiltrationComboBoxFilterListVM FilterList { get; }
    public StringParamSetService StrParamSetService { get; }

    public string ValueFormula {
        get => _valueFormula;
        set => RaiseAndSetIfChanged(ref _valueFormula, value);
    }

    public string Value {
        get => _value;
        set => RaiseAndSetIfChanged(ref _value, value);
    }

    private void PropUpdateByFormula(string formulaPropertyName) {
        StrParamSetService.Set(this, formulaPropertyName, FilterList.SheetSetParams);
    }

    /// <summary>
    /// В случае изменения имени параметра конфигурации нужно обновить свойства дополнительного параметра
    /// </summary>
    public void UpdateDueParamNameChange() {
        StrParamSetService.SetAll(this, FilterList.SheetSetParams);
    }

    public void UpdateDueParamValueChange(StringParamVM stringParam) {
        StrParamSetService.SetAll(this, FilterList.SheetSetParams, stringParam);
    }
}
