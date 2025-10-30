using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitCreateViewSheet.ViewModels {
    internal class ViewScheduleViewModel : BaseViewModel, IEquatable<ViewScheduleViewModel> {
#if REVIT_2022_OR_GREATER
        public ViewScheduleViewModel(ViewSchedule viewSchedule, int segmentIndex = -1) {
            ViewSchedule = viewSchedule ?? throw new ArgumentNullException(nameof(viewSchedule));
            int count = viewSchedule.GetSegmentCount();
            if(segmentIndex >= count) {
                throw new ArgumentException(nameof(segmentIndex));
            }
            SegmentIndex = segmentIndex;
            Name = segmentIndex > -1
                ? ViewSchedule.Name + $" {SegmentIndex + 1}/{count}"
                : ViewSchedule.Name;
        }
#else
        public ViewScheduleViewModel(ViewSchedule viewSchedule) {
            ViewSchedule = viewSchedule ?? throw new ArgumentNullException(nameof(viewSchedule));
            Name = ViewSchedule.Name;
        }
#endif
        public string Name { get; }

        public ViewSchedule ViewSchedule { get; }

#if REVIT_2022_OR_GREATER
        public int SegmentIndex { get; }
#endif
        public bool Equals(ViewScheduleViewModel other) {
            if(other is null) { return false; }
            if(ReferenceEquals(this, other)) { return true; }
            return ViewSchedule.Id == other.ViewSchedule.Id
                && Name == other.Name;
        }

        public override int GetHashCode() {
            int hashCode = -1702170149;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementId>.Default.GetHashCode(ViewSchedule.Id);
            return hashCode;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ViewScheduleViewModel);
        }
    }
}
