using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ScheduleFilters;
using RevitPackageDocumentation.ViewModels.Parameters;

namespace RevitPackageDocumentation.ViewModels.ScheduleFilters;
internal class ScheduleFilterRuleVM : BaseViewModel {
    private ScheduleFieldInfo _selectedSpecField;
    private string _selectedSpecFieldName;
    private ScheduleTypeInfo _selectedFilterType;

    private string _filterValueFormula = string.Empty;
    private string _filterValue;

    public ScheduleFilterRuleVM(ScheduleFilterListVM scheduleFilterListVM, StringParamSetService stringParamSetService) {
        ScheduleFilterList = scheduleFilterListVM;
        StrParamSetService = stringParamSetService;

        SelectSpecFieldCommand = RelayCommand.Create(SelectSpecField);
        PropUpdateByFormulaCommand = RelayCommand.Create<string>(PropUpdateByFormula);
    }

    public ICommand SelectSpecFieldCommand { get; }
    public ICommand PropUpdateByFormulaCommand { get; }

    public ScheduleFilterListVM ScheduleFilterList { get; }
    public StringParamSetService StrParamSetService { get; }

    public ScheduleFieldInfo SelectedSpecField {
        get => _selectedSpecField;
        set => RaiseAndSetIfChanged(ref _selectedSpecField, value);
    }

    public string SelectedSpecFieldName {
        get => _selectedSpecFieldName;
        set => RaiseAndSetIfChanged(ref _selectedSpecFieldName, value);
    }

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
        StrParamSetService.Set(this, formulaPropertyName, ScheduleFilterList.ScheduleView.Sheet.SheetSet.Params);
    }

    public void UpdateDueParamValueChange(StringParamVM stringParam) {
        StrParamSetService.SetAll(this, ScheduleFilterList.ScheduleView.Sheet.SheetSet.Params, stringParam);
    }
}
