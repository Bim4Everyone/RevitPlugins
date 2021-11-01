using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitRooms.ViewModels {
    internal class PhaseViewModel : BaseViewModel, IComparable<PhaseViewModel>, IEquatable<PhaseViewModel> {
        private readonly Phase _phase;

        public PhaseViewModel(Phase phase) {
            _phase = phase;
        }

        public Phase Element {
            get { return _phase; }
        }

        public string DisplayData {
            get { return _phase.Name; }
        }

        #region SystemOverrides

        public int CompareTo(PhaseViewModel other) {
            return _phase.Name.CompareTo(other._phase.Name);
        }

        public override bool Equals(object obj) {
            return Equals(obj as PhaseViewModel);
        }

        public bool Equals(PhaseViewModel other) {
            return other != null && _phase.Id.Equals(other._phase.Id);
        }

        public override int GetHashCode() {
            return -2121273300 + _phase.Id.GetHashCode();
        }

        #endregion
    }
}
