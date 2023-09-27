using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;

using RevitRoomTagPlacement.Models;

namespace RevitRoomTagPlacement.ViewModels {
    internal class ViewRevitViewModel : RevitViewModel {
        public ViewRevitViewModel(RevitRepository revitRepository) 
            : base(revitRepository) {
        }

        protected override BindingList<RoomGroupViewModel> GetGroupViewModels() {
            var roomGroupsList = _revitRepository.GetRoomsOnActiveView()
                .GroupBy(x => x.GetParam(ProjectParamsConfig.Instance.RoomGroupName).AsValueString())
                .Select(x => new RoomGroupViewModel(x.Key.ToString(), x))
                .ToList();

            return new BindingList<RoomGroupViewModel>(roomGroupsList);
        }
    }
}
