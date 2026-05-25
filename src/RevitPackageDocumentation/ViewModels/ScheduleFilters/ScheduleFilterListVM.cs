using System.Collections.ObjectModel;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

namespace RevitPackageDocumentation.ViewModels.ScheduleFilters;
internal class ScheduleFilterListVM : BaseViewModel {
    private ObservableCollection<ScheduleFilterRuleVM> _scheduleFilterRules = [];

    public ScheduleFilterListVM(ScheduleViewVM scheduleViewVM) {
        ScheduleView = scheduleViewVM;

        AddFilterCommand = RelayCommand.Create(AddFilter);
        RemoveFilterCommand = RelayCommand.Create<ScheduleFilterRuleVM>(RemoveFilter);
    }

    public ICommand AddFilterCommand { get; }
    public ICommand RemoveFilterCommand { get; }

    public ScheduleViewVM ScheduleView { get; }

    public ObservableCollection<ScheduleFilterRuleVM> ScheduleFilterRules {
        get => _scheduleFilterRules;
        set => RaiseAndSetIfChanged(ref _scheduleFilterRules, value);
    }

    private void AddFilter() {
        var rule = new ScheduleFilterRuleVM(this);
        rule.SetSchedule(ScheduleView.ReferenceSpec);
        ScheduleFilterRules.Add(rule);
    }

    private void RemoveFilter(ScheduleFilterRuleVM rule) {
        ScheduleFilterRules.Remove(rule);
    }

    internal void SetSchedule(ViewSchedule schedule) {
        foreach(var rule in ScheduleFilterRules) {
            rule.SetSchedule(schedule);
        }
    }
}
