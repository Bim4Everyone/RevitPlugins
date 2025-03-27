using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitCreateViewSheet.Models {
    internal class ScheduleModel : IModel, IEquatable<ScheduleModel> {
        private readonly ScheduleSheetInstance _scheduleInstance;
        private readonly ViewSchedule _schedule;
        private XYZ _location;

        public ScheduleModel(SheetModel sheet, ScheduleSheetInstance scheduleInstance) {
            Sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            _scheduleInstance = scheduleInstance ?? throw new ArgumentNullException(nameof(scheduleInstance));
            _location = scheduleInstance.Point;
            State = EntityState.Unchanged;
        }

        public ScheduleModel(SheetModel sheet, ViewSchedule schedule) {
            Sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            _schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
            State = EntityState.Added;
        }


        public EntityState State { get; private set; }

        public SheetModel Sheet { get; }


        public void SetLocation(XYZ point) {
            _location = point ?? throw new ArgumentNullException(nameof(point));
        }

        public void MarkAsDeleted() {
            State = EntityState.Deleted;
        }

        public void SaveChanges(RevitRepository repository) {
            if(State == EntityState.Deleted && _scheduleInstance is not null) {
                repository.RemoveElement(_scheduleInstance.Id);
            } else if(State == EntityState.Added && _schedule is not null) {
                if(_location is null) {
                    throw new InvalidOperationException("Сначала необходимо назначить Location спецификации");
                }
                repository.CreateSchedule(Sheet.GetViewSheet().Id, _schedule.Id, _location);
                State = EntityState.Unchanged;
            }
        }

        public bool Equals(ScheduleModel other) {
            return other is not null
                && Sheet.Equals(other.Sheet)
                && (_scheduleInstance?.Id == other._scheduleInstance?.Id);
        }

        public override bool Equals(object obj) {
            return Equals(obj as ScheduleModel);
        }

        public override int GetHashCode() {
            int hashCode = -43849488;
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementId>.Default.GetHashCode(_scheduleInstance?.Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<SheetModel>.Default.GetHashCode(Sheet);
            return hashCode;
        }
    }
}
