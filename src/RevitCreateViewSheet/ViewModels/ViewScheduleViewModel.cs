using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitCreateViewSheet.ViewModels {
    internal class ViewScheduleViewModel : BaseViewModel, IEquatable<ViewScheduleViewModel> {
        private readonly ViewSchedule _viewSchedule;

        public ViewScheduleViewModel(ViewSchedule viewSchedule) {
            _viewSchedule = viewSchedule ?? throw new ArgumentNullException(nameof(viewSchedule));
        }

        public string Name => _viewSchedule.Name;

        public ViewSchedule ViewSchedule => _viewSchedule;

        public bool Equals(ViewScheduleViewModel other) {
            return other is not null
                && _viewSchedule.Id == other._viewSchedule.Id;
        }

        public override int GetHashCode() {
            return -1826915298 + EqualityComparer<ElementId>.Default.GetHashCode(_viewSchedule.Id);
        }

        public override bool Equals(object obj) {
            return Equals(obj as ViewScheduleViewModel);
        }
    }
}
