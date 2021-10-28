using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitRooms.ViewModels {
    internal class PhaseViewModel : BaseViewModel {
        private readonly Phase _phase;

        public PhaseViewModel(Phase phase) {
            _phase = phase;
        }

        public string DisplayData {
            get { return _phase.Name; }
        }
    }
}
