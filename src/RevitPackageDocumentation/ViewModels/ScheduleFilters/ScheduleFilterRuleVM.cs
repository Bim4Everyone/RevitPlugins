using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models.ScheduleFilters;

namespace RevitPackageDocumentation.ViewModels.ScheduleFilters;
internal class ScheduleFilterRuleVM : BaseViewModel {
    private ObservableCollection<ScheduleFieldInfo> _specFields = [];
    private ScheduleFieldInfo _selectedSpecField;
    private ScheduleTypeInfo _selectedFilterType;
    private string _filterValue = "";
    private string _selectedSpecFieldName;

    public ScheduleFilterRuleVM(ScheduleFilterListVM scheduleFilterListVM) {
        ScheduleFilterList = scheduleFilterListVM;
        SelectSpecFieldCommand = RelayCommand.Create(SelectSpecField);
    }

    public ICommand SelectSpecFieldCommand { get; }


    public ScheduleFilterListVM ScheduleFilterList { get; }

    public ObservableCollection<ScheduleFieldInfo> SpecFields {
        get => _specFields;
        set => RaiseAndSetIfChanged(ref _specFields, value);
    }

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

    public string FilterValue {
        get => _filterValue;
        set => RaiseAndSetIfChanged(ref _filterValue, value);
    }

    private void SelectSpecField() {
        if(SelectedSpecField != null) {
            SelectedSpecFieldName = SelectedSpecField.FieldName ?? string.Empty;
        }
    }

    public void SetSchedule(ViewSchedule viewSchedule) {
        if(viewSchedule is null) {
            return;
        }
        var scheduleDefinition = viewSchedule.Definition;

        SpecFields.Clear();
        foreach(var fieldId in scheduleDefinition.GetFieldOrder()) {
            SpecFields.Add(new ScheduleFieldInfo(scheduleDefinition.GetField(fieldId)));
        }
        SelectedSpecField = SpecFields.FirstOrDefault(f => f.FieldName == SelectedSpecFieldName);
    }
}
