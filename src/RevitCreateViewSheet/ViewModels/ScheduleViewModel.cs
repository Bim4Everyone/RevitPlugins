using System;
using System.Collections.Generic;

using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class ScheduleViewModel : BaseViewModel, IEquatable<ScheduleViewModel> {
        private readonly ScheduleModel _scheduleModel;

        public ScheduleViewModel(ScheduleModel scheduleModel) {
            _scheduleModel = scheduleModel ?? throw new ArgumentNullException(nameof(scheduleModel));
            IsPlaced = scheduleModel.State == EntityState.Unchanged;
        }


        public string Name => _scheduleModel.Name;

        public bool IsPlaced { get; }

        public ScheduleModel ScheduleModel => _scheduleModel;


        public bool Equals(ScheduleViewModel other) {
            return other is not null
                && _scheduleModel.Equals(other._scheduleModel);
        }

        public override bool Equals(object obj) {
            return Equals(obj as ScheduleViewModel);
        }

        public override int GetHashCode() {
            return 539060726 + EqualityComparer<ScheduleModel>.Default.GetHashCode(_scheduleModel);
        }
    }
}
