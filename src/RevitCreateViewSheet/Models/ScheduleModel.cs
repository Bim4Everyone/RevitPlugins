using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitCreateViewSheet.Services;

namespace RevitCreateViewSheet.Models {
    internal class ScheduleModel : IEntity, IEquatable<ScheduleModel> {
        private readonly ScheduleSheetInstance _scheduleInstance;

        /// <summary>
        /// Создает модель размещенной на листе спецификации
        /// </summary>
        /// <param name="sheet">Модель листа</param>
        /// <param name="scheduleInstance">Размещенный на листе экземпляр спецификации</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public ScheduleModel(SheetModel sheet, ScheduleSheetInstance scheduleInstance, ExistsEntitySaver entitySaver) {
            Sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            _scheduleInstance = scheduleInstance ?? throw new ArgumentNullException(nameof(scheduleInstance));
            Saver = entitySaver ?? throw new ArgumentNullException(nameof(entitySaver));
            ViewSchedule = _scheduleInstance.Document.GetElement(_scheduleInstance.ScheduleId) as ViewSchedule
                ?? throw new ArgumentNullException(nameof(_scheduleInstance.ScheduleId));
            Location = scheduleInstance.Point;
            Name = scheduleInstance.Name;
            Exists = true;
#if REVIT_2022_OR_GREATER
            SegmentIndex = scheduleInstance.SegmentIndex;
#endif
        }

#if REVIT_2022_OR_GREATER
        /// <summary>
        /// Создает модель новой спецификации на листе
        /// </summary>
        /// <param name="sheet">Модель листа</param>
        /// <param name="schedule">Спецификация</param>
        /// <param name="entitySaver">Сервис для сохранения новых элементов</param>
        /// <param name="segmentIndex">Индекс сегмента спецификации</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public ScheduleModel(SheetModel sheet, ViewSchedule schedule, NewEntitySaver entitySaver, int segmentIndex = -1) {
            Sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            ViewSchedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
            Saver = entitySaver ?? throw new ArgumentNullException(nameof(entitySaver));
            int count = ViewSchedule.GetSegmentCount();
            if(segmentIndex >= count) {
                throw new ArgumentException(nameof(segmentIndex));
            }
            SegmentIndex = segmentIndex;
            Name = SegmentIndex > -1
                ? ViewSchedule.Name + $" {SegmentIndex + 1}/{count}"
                : ViewSchedule.Name;
            Exists = false;
        }
#else
        /// <summary>
        /// Создает модель новой спецификации на листе
        /// </summary>
        /// <param name="sheet">Модель листа</param>
        /// <param name="schedule">Спецификация</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public ScheduleModel(SheetModel sheet, ViewSchedule schedule, NewEntitySaver entitySaver) {
            Sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            ViewSchedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
            Saver = entitySaver ?? throw new ArgumentNullException(nameof(entitySaver));
            Name = schedule.Name;
            Exists = false;
        }
#endif


        public bool Exists { get; }

        public XYZ Location { get; set; }

        public SheetModel Sheet { get; }

        public ViewSchedule ViewSchedule { get; }

        public string Name { get; }

#if REVIT_2022_OR_GREATER
        public int SegmentIndex { get; }
#endif

        public IEntitySaver Saver { get; }


        public bool TryGetExistId(out ElementId id) {
            id = Exists ? _scheduleInstance.Id : null;
            return Exists;
        }

        public bool TryGetScheduleInstance(out ScheduleSheetInstance instance) {
            if(Exists && _scheduleInstance is null) {
                throw new InvalidOperationException();
            }
            instance = Exists ? _scheduleInstance : null;
            return Exists;
        }

        public bool Equals(ScheduleModel other) {
            if(other is null) { return false; }
            if(ReferenceEquals(this, other)) { return true; }
            return Sheet.Equals(other.Sheet)
                && (_scheduleInstance?.Id == other._scheduleInstance?.Id)
                && Name == other.Name;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ScheduleModel);
        }

        public override int GetHashCode() {
            int hashCode = -43849488;
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementId>.Default.GetHashCode(_scheduleInstance?.Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<SheetModel>.Default.GetHashCode(Sheet);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }
    }
}
