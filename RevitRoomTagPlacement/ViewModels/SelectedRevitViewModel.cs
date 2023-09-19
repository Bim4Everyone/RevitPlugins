using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitRoomTagPlacement.Models;

namespace RevitRoomTagPlacement.ViewModels {
    internal class SelectedRevitViewModel : RevitViewModel {
        public SelectedRevitViewModel(RevitRepository revitRepository) : base(revitRepository) {
        }
    }
}
