using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ScheduleFilters;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.Validation;

namespace RevitPackageDocumentation.ViewModels.ScheduleFilters;
internal class ScheduleFilterRuleVM : ValidatableVM {
    private ScheduleFieldInfo _selectedSpecField;
    private string _selectedSpecFieldName;
    private ScheduleTypeInfo _selectedFilterType;

    private string _filterValueFormula = string.Empty;
    private string _filterValue;

    public ScheduleFilterRuleVM(
        ScheduleFilterListVM scheduleFilterListVM,
        StringParamSetService stringParamSetService,
        ILocalizationService localizationService)
        : base(localizationService) {

        ScheduleFilterList = scheduleFilterListVM;
        StrParamSetService = stringParamSetService;
        ValidateAllProperties();

        SelectSpecFieldCommand = RelayCommand.Create(SelectSpecField);
        PropUpdateByFormulaCommand = RelayCommand.Create<string>(PropUpdateByFormula);
    }

    public ICommand SelectSpecFieldCommand { get; }
    public ICommand PropUpdateByFormulaCommand { get; }

    public ScheduleFilterListVM ScheduleFilterList { get; }
    public StringParamSetService StrParamSetService { get; }

    [Required]
    public ScheduleFieldInfo SelectedSpecField {
        get => _selectedSpecField;
        set => RaiseAndSetIfChanged(ref _selectedSpecField, value);
    }

    public string SelectedSpecFieldName {
        get => _selectedSpecFieldName;
        set => RaiseAndSetIfChanged(ref _selectedSpecFieldName, value);
    }

    [Required]
    public ScheduleTypeInfo SelectedFilterType {
        get => _selectedFilterType;
        set => RaiseAndSetIfChanged(ref _selectedFilterType, value);
    }

    public string FilterValueFormula {
        get => _filterValueFormula;
        set => RaiseAndSetIfChanged(ref _filterValueFormula, value);
    }

    public string FilterValue {
        get => _filterValue;
        set => RaiseAndSetIfChanged(ref _filterValue, value);
    }

    private void SelectSpecField() {
        if(SelectedSpecField != null) {
            SelectedSpecFieldName = SelectedSpecField.FieldName ?? string.Empty;
        }
    }

    private void PropUpdateByFormula(string formulaPropertyName) {
        StrParamSetService.Set(this, formulaPropertyName, ScheduleFilterList.ScheduleView.Sheet.SheetSet.SheetSetParams.Params);
    }

    /// <summary>
    /// В случае изменения имени параметра конфигурации нужно обновить свойства дополнительного параметра
    /// </summary>
    public void UpdateDueParamNameChange() {
        StrParamSetService.SetAll(this, ScheduleFilterList.ScheduleView.Sheet.SheetSet.SheetSetParams.Params);
    }

    public void UpdateDueParamValueChange(StringParamVM stringParam) {
        StrParamSetService.SetAll(this, ScheduleFilterList.ScheduleView.Sheet.SheetSet.SheetSetParams.Params, stringParam);
    }
}
