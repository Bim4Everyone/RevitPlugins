using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoomTagPlacement.Models;

namespace RevitRoomTagPlacement.ViewModels {
    
    
    internal abstract class RevitViewModel : BaseViewModel {
        protected readonly RevitRepository _revitRepository;

        public RevitViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;

            RoomGroups = new ObservableCollection<RoomGroupViewModel>(GetGroupViewModels());
        }
        public string Name { get; set; }

        public ObservableCollection<RoomGroupViewModel> RoomGroups { get; }

        protected abstract IEnumerable<RoomGroupViewModel> GetGroupViewModels();
    }

}
