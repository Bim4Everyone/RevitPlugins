using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitExportSpecToJson.Models;

namespace RevitExportSpecToJson.ViewModels;

internal sealed class ScheduleViewModel : BaseViewModel {
    public ScheduleViewModel(Schedule schedule) {
        Schedule = schedule;
        TypeSchedule = Schedule.TypeSchedule;
    }

    public Schedule Schedule { get; }

    public ElementId Id => Schedule.Id;
    public string Name => Schedule.Name;

    public ScheduleStatus Status => Schedule.Status;

    public bool Checked {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? TypeSchedule {
        get;
        set {
            Schedule.TypeSchedule = value;
            this.RaiseAndSetIfChanged(ref field, value);
        }
    }
}
