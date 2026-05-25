using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;

namespace RevitPackageDocumentation.ViewModels.ScheduleFilters;
internal class ScheduleFilterRuleVM : BaseViewModel {
    private ObservableCollection<ScheduleFieldInfo> _specFields = [];
    private ScheduleFieldInfo _selectedSpecField;
    private IList<ScheduleFilterType> _filterRules;
    private ScheduleFilterType _selectedFilterRules;
    private string _filterValue;
    private string _selectedSpecFieldName;

    public ScheduleFilterRuleVM(ScheduleFilterListVM scheduleFilterListVM) {
        ScheduleFilterList = scheduleFilterListVM;

        FilterRules = Enum.GetValues(typeof(ScheduleFilterType))
            .Cast<ScheduleFilterType>()
            .ToList();

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

    public IList<ScheduleFilterType> FilterRules {
        get => _filterRules;
        set => RaiseAndSetIfChanged(ref _filterRules, value);
    }

    public ScheduleFilterType SelectedFilterRules {
        get => _selectedFilterRules;
        set => RaiseAndSetIfChanged(ref _selectedFilterRules, value);
    }

    public string FilterValue {
        get => _filterValue;
        set => RaiseAndSetIfChanged(ref _filterValue, value);
    }

    private void SelectSpecField() {
        if(SelectedSpecField != null) {
            _selectedSpecFieldName = SelectedSpecField.FieldName;
        }
    }

    public void SetSchedule(ViewSchedule viewSchedule) {
        var scheduleDefinition = viewSchedule.Definition;

        SpecFields.Clear();
        foreach(var fieldId in scheduleDefinition.GetFieldOrder()) {
            SpecFields.Add(new ScheduleFieldInfo(scheduleDefinition.GetField(fieldId)));
        }
        SelectedSpecField = SpecFields.FirstOrDefault(f => f.FieldName == _selectedSpecFieldName);
    }
}
