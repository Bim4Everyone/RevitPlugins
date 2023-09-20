using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;

using RevitRoomTagPlacement.Models;

namespace RevitRoomTagPlacement.ViewModels {
    internal class SelectedRevitViewModel : RevitViewModel {
        public SelectedRevitViewModel(RevitRepository revitRepository) : base(revitRepository) {
        }

        protected override IEnumerable<RoomGroupViewModel> GetGroupViewModels() {
            var selectedRooms = _revitRepository.GetSelectedRooms();

            return selectedRooms
                .GroupBy(x => x.GetParam(ProjectParamsConfig.Instance.RoomGroupName).AsValueString())
                .Select(x => new RoomGroupViewModel(x.Key.ToString(), x));
        }
    }
}
