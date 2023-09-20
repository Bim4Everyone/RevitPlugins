using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoomTagPlacement.Models;

namespace RevitRoomTagPlacement.ViewModels {
    
    
    internal abstract class RevitViewModel : BaseViewModel {
        protected readonly RevitRepository _revitRepository;

        public RevitViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;

            RoomGroups = new ObservableCollection<RoomGroupViewModel>(GetGroupViewModels());

            Place = new RelayCommand(PlaceTags, CanPlaceTags);
        }
        public string Name { get; set; }

        public ICommand Place { get; }

        public ObservableCollection<RoomGroupViewModel> RoomGroups { get; }

        protected abstract IEnumerable<RoomGroupViewModel> GetGroupViewModels();

        private void PlaceTags(object p) {
            _revitRepository.PlaceTagsCommand(RoomGroups);
        }

        private bool CanPlaceTags(object p) {
            return true;
        }
    }

}
