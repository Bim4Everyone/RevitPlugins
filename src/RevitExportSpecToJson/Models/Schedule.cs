using Autodesk.Revit.DB;

namespace RevitExportSpecToJson.Models;

internal enum ScheduleStatus {
    Active,
    Opened,
    Closed,
}

internal sealed class Schedule {
    public Schedule(ViewSchedule viewSchedule) {
        ViewSchedule = viewSchedule;
    }
    
    public ViewSchedule ViewSchedule { get; }

    public ElementId Id => ViewSchedule.Id;
    public string Name => ViewSchedule.Name;

    public string? TypeSchedule { get; set; }
    public ScheduleStatus Status { get; set; } = ScheduleStatus.Closed;
}
