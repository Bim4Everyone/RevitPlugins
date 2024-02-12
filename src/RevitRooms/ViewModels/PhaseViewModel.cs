using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal class PhaseViewModel : ElementViewModel<Phase> {
        public PhaseViewModel(Phase phase, RevitRepository revitRepository)
            : base(phase, revitRepository) {
        }
    }
}
