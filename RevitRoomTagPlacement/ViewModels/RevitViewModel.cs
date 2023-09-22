using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoomTagPlacement.Models;

namespace RevitRoomTagPlacement.ViewModels {
    
    
    internal abstract class RevitViewModel : BaseViewModel {
        protected readonly RevitRepository _revitRepository;

        private RoomTagType _selectedTagType;

        public RevitViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;

            RoomGroups = new ObservableCollection<RoomGroupViewModel>(GetGroupViewModels());

            PlaceCommand = new RelayCommand(PlaceTags, CanPlaceTags);

            TagFamilies = revitRepository.GetRoomTags();

            //SelectedTagType = TagFamilies[1].Id;
        }
        public string Name { get; set; }

        public ICommand PlaceCommand { get; }

        public ObservableCollection<RoomGroupViewModel> RoomGroups { get; }

        public List<RoomTagType> TagFamilies { get; }

        protected abstract IEnumerable<RoomGroupViewModel> GetGroupViewModels();

        public RoomTagType SelectedTagType {
            get => _selectedTagType;
            set => this.RaiseAndSetIfChanged(ref _selectedTagType, value);
        }

        private void PlaceTags(object p) {
            _revitRepository.PlaceTagsCommand(RoomGroups, SelectedTagType.Id);
        }

        private bool CanPlaceTags(object p) {
            return true;
        }
    }

}
