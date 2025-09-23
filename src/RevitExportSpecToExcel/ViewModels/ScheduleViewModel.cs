using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitExportSpecToExcel.Models;

namespace RevitExportSpecToExcel.ViewModels;

internal class ScheduleViewModel : BaseViewModel {
    private readonly string _name;
    private readonly OpenStatus _openStatus;
    private readonly ViewSchedule _schedule;
    private bool _isChecked;

    public ScheduleViewModel(ViewSchedule schedule, OpenStatus status) {
        _schedule = schedule;
        _name = schedule.Name;
        _openStatus = status;
    }

    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }

    public string Name => _name;
    public OpenStatus OpenStatus => _openStatus;
    public ViewSchedule Schedule => _schedule;
}

