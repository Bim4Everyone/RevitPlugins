using System;

using Autodesk.Revit.DB;

namespace RevitBatchSpecExport.Models;

/// <summary>
/// Видовой экран спецификации, которая размещена на листе
/// </summary>
public class ScheduleModel : IEquatable<ScheduleModel> {
    private readonly ElementId _scheduleId;

    public ScheduleModel(ScheduleSheetInstance schedule) {
        if(schedule == null) {
            throw new ArgumentNullException(nameof(schedule));
        }

        OwnerViewId = schedule.OwnerViewId;
        _scheduleId = schedule.ScheduleId;
        Schedule = (ViewSchedule) schedule.Document.GetElement(schedule.ScheduleId);
    }

    /// <summary>
    /// Id листа, на котором размещен экземпляр спецификации с видового экрана спецификации
    /// </summary>
    public ElementId OwnerViewId { get; }

    /// <summary>
    /// Видовой экран спецификации Revit
    /// </summary>
    public ViewSchedule Schedule { get; }

    public bool Equals(ScheduleModel other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return Equals(_scheduleId, other._scheduleId);
    }

    public override bool Equals(object obj) {
        if(obj is null) {
            return false;
        }

        if(ReferenceEquals(this, obj)) {
            return true;
        }

        if(obj.GetType() != GetType()) {
            return false;
        }

        return Equals((ScheduleModel) obj);
    }

    public override int GetHashCode() {
        return (_scheduleId != null ? _scheduleId.GetHashCode() : 0);
    }
}
