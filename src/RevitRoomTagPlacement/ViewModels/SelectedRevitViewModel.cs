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
    internal class SelectedRevitViewModel : RevitViewModel {
        public SelectedRevitViewModel(RevitRepository revitRepository) 
            : base(revitRepository) {
        }

        protected override BindingList<RoomGroupViewModel> GetGroupViewModels() {
            RevitParam groupParam = ProjectParamsConfig.Instance.RoomGroupName;
            
            var selectedRoomsList = _revitRepository.GetSelectedRooms()
                .Where(r => r.RoomObject.Area > 0)
                .GroupBy(x => x.RoomObject.GetParamValueStringOrDefault(groupParam, "<Без группы>"))
                .Select(x => new RoomGroupViewModel(x.Key.ToString(), x))
                .OrderBy(x => x.Name)
                .ToList();

            return new BindingList<RoomGroupViewModel>(selectedRoomsList);
        }
    }
}
