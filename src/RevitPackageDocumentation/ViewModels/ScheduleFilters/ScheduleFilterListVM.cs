using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ScheduleFilters;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
using RevitPackageDocumentation.ViewModels.Validation;
using RevitPackageDocumentation.ViewModels.Validation.Attributes;

namespace RevitPackageDocumentation.ViewModels.ScheduleFilters;
internal class ScheduleFilterListVM : ValidatableVM {
    private readonly ILocalizationService _localizationService;
    private ObservableCollection<ScheduleFilterRuleVM> _scheduleFilterRules = [];
    private ObservableCollection<ScheduleFieldInfo> _selectedScheduleFields = [];

    public ScheduleFilterListVM(
        ScheduleViewVM scheduleViewVM,
        StringParamSetService stringParamSetService,
        ILocalizationService localizationService)
        : base(localizationService) {

        ScheduleView = scheduleViewVM;
        StrParamSetService = stringParamSetService;
        _localizationService = localizationService;
        ValidateAllProperties();

        AddFilterCommand = RelayCommand.Create(AddFilter);
        RemoveFilterCommand = RelayCommand.Create<ScheduleFilterRuleVM>(RemoveFilter);
    }

    public ICommand AddFilterCommand { get; }
    public ICommand RemoveFilterCommand { get; }

    public ScheduleViewVM ScheduleView { get; }
    public StringParamSetService StrParamSetService { get; }

    [ChildHasErrors]
    public ObservableCollection<ScheduleFilterRuleVM> ScheduleFilterRules {
        get => _scheduleFilterRules;
        set => RaiseAndSetIfChanged(ref _scheduleFilterRules, value);
    }

    public ObservableCollection<ScheduleFieldInfo> SelectedScheduleFields {
        get => _selectedScheduleFields;
        set => RaiseAndSetIfChanged(ref _selectedScheduleFields, value);
    }

    private void AddFilter() {
        var rule = new ScheduleFilterRuleVM(this, StrParamSetService, _localizationService);
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

            string filterValue = GetFilterValue(definition, scheduleFilter);

            var ruleVM = new ScheduleFilterRuleVM(this, StrParamSetService, _localizationService) {
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

    /// <summary>
    /// Возвращает значение фильтра спецификации в виде, понятном пользователю.
    /// Для рабочего набора по условию "равно"/"не равно" в фильтре хранится id набора - показываем его имя
    /// </summary>
    private string GetFilterValue(ScheduleDefinition definition, ScheduleFilter scheduleFilter) {
        var fieldId = scheduleFilter.FieldId;
        try {
            if(ScheduleView.Repository.IsWorksetField(definition, fieldId)) {
                return scheduleFilter.IsStringValue
                    ? scheduleFilter.GetStringValue() ?? string.Empty
                    : ScheduleView.Repository.GetWorksetName(scheduleFilter.GetIntegerValue()) ?? string.Empty;
            }
            if(definition.CanFilterBySubstring(fieldId)) {
                return scheduleFilter.GetStringValue() ?? string.Empty;
            }
            return scheduleFilter.GetIntegerValue().ToString();
        } catch(Exception) {
            return string.Empty;
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
