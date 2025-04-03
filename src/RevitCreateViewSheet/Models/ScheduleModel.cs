using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitCreateViewSheet.Models {
    internal class ScheduleModel : IEntity, IEquatable<ScheduleModel> {
        private readonly ScheduleSheetInstance _scheduleInstance;
        private readonly ViewSchedule _schedule;

        /// <summary>
        /// Создает модель размещенной на листе спецификации
        /// </summary>
        /// <param name="sheet">Модель листа</param>
        /// <param name="scheduleInstance">Размещенный на листе экземпляр спецификации</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public ScheduleModel(SheetModel sheet, ScheduleSheetInstance scheduleInstance) {
            Sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            _scheduleInstance = scheduleInstance ?? throw new ArgumentNullException(nameof(scheduleInstance));
            Location = scheduleInstance.Point;
            Name = scheduleInstance.Name;
            State = EntityState.Unchanged;
        }

        /// <summary>
        /// Создает модель новой спецификации на листе
        /// </summary>
        /// <param name="sheet">Модель листа</param>
        /// <param name="schedule">Спецификация</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public ScheduleModel(SheetModel sheet, ViewSchedule schedule) {
            Sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            _schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
            Name = schedule.Name;
            State = EntityState.Added;
        }


        public EntityState State { get; private set; }

        public XYZ Location { get; set; }

        public SheetModel Sheet { get; }

        public string Name { get; }


        public void MarkAsDeleted() {
            State = EntityState.Deleted;
        }

        public void SaveChanges(RevitRepository repository) {
            if(State == EntityState.Deleted && _scheduleInstance is not null) {
                repository.DeleteElement(_scheduleInstance.Id);
            } else if(State == EntityState.Added && _schedule is not null) {
                if(Location is null) {
                    throw new InvalidOperationException($"Перед сохранением необходимо назначить {nameof(Location)}");
                }
                repository.CreateSchedule(Sheet.GetViewSheet().Id, _schedule.Id, Location);
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
