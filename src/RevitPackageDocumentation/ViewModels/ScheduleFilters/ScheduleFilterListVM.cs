using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ScheduleFilters;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

namespace RevitPackageDocumentation.ViewModels.ScheduleFilters;
internal class ScheduleFilterListVM : BaseViewModel {
    private ObservableCollection<ScheduleFilterRuleVM> _scheduleFilterRules = [];
    private ObservableCollection<ScheduleFieldInfo> _selectedScheduleFields = [];

    public ScheduleFilterListVM(ScheduleViewVM scheduleViewVM, StringParamSetService stringParamSetService) {
        ScheduleView = scheduleViewVM;
        StrParamSetService = stringParamSetService;

        AddFilterCommand = RelayCommand.Create(AddFilter);
        RemoveFilterCommand = RelayCommand.Create<ScheduleFilterRuleVM>(RemoveFilter);
    }

    public ICommand AddFilterCommand { get; }
    public ICommand RemoveFilterCommand { get; }

    public ScheduleViewVM ScheduleView { get; }
    public StringParamSetService StrParamSetService { get; }

    public ObservableCollection<ScheduleFilterRuleVM> ScheduleFilterRules {
        get => _scheduleFilterRules;
        set => RaiseAndSetIfChanged(ref _scheduleFilterRules, value);
    }

    public ObservableCollection<ScheduleFieldInfo> SelectedScheduleFields {
        get => _selectedScheduleFields;
        set => RaiseAndSetIfChanged(ref _selectedScheduleFields, value);
    }

    private void AddFilter() {
        var rule = new ScheduleFilterRuleVM(this, StrParamSetService);
        ScheduleFilterRules.Add(rule);
    }

    private void RemoveFilter(ScheduleFilterRuleVM rule) {
        ScheduleFilterRules.Remove(rule);
    }

    internal void SetSchedule(ViewSchedule schedule) {
        ScheduleFilterRules.Clear();
        SelectedScheduleFields.Clear();

        if(schedule == null) {
            return;
        }
        var definition = schedule.Definition;

        foreach(var fieldId in definition.GetFieldOrder()) {
            SelectedScheduleFields.Add(new ScheduleFieldInfo(definition.GetField(fieldId)));
        }

        foreach(var scheduleFilter in definition.GetFilters()) {
            var fieldId = scheduleFilter.FieldId;
            var scheduleFilterField = definition.GetField(fieldId);
            var selectedSpecField =
                SelectedScheduleFields.FirstOrDefault(f => f.Field.FieldId == scheduleFilterField.FieldId);

            string filterValue = string.Empty;
            try {
                if(definition.CanFilterBySubstring(fieldId)) {
                    filterValue = scheduleFilter.GetStringValue() ?? string.Empty;
                } else {
                    filterValue = scheduleFilter.GetIntegerValue().ToString() ?? string.Empty;
                }
            } catch(Exception) { }

            var ruleVM = new ScheduleFilterRuleVM(this, StrParamSetService) {
                SelectedSpecField = selectedSpecField,
                SelectedSpecFieldName = selectedSpecField.FieldName,
                SelectedFilterType =
                    ScheduleView.Repository.FilterTypes.FirstOrDefault(f => f.FilterType == scheduleFilter.FilterType),
                FilterValueFormula = filterValue,
                FilterValue = filterValue,
            };
            ScheduleFilterRules.Add(ruleVM);
        }
    }

    internal void SetScheduleToRemember(ViewSchedule schedule) {
        if(schedule == null) {
            return;
        }
        var definition = schedule.Definition;

        foreach(var fieldId in definition.GetFieldOrder()) {
            SelectedScheduleFields.Add(new ScheduleFieldInfo(definition.GetField(fieldId)));
        }

        foreach(var rule in ScheduleFilterRules) {
            rule.SelectedSpecField = SelectedScheduleFields.FirstOrDefault(f => f.FieldName == rule.SelectedSpecFieldName);
        }
    }
}
