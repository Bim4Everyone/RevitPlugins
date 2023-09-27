using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private RoomTagTypeModel _selectedTagType;
        private GroupPlacementWay placementWayByGroups = GroupPlacementWay.EveryRoom;
        private ObservableCollection<string> roomNames;

        public RevitViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            RoomGroups = new BindingList<RoomGroupViewModel>(GetGroupViewModels());
            PlaceCommand = new RelayCommand(PlaceTags, CanPlaceTags);
            TagFamilies = revitRepository.GetRoomTags();
            SelectedTagType = TagFamilies[0];
            RoomNames = new ObservableCollection<string>();
            RoomGroups.ListChanged += RoomGroups_ListChanged;
        }

        private void RoomGroups_ListChanged(object sender, ListChangedEventArgs e) {
            RoomNames = _revitRepository.GetRoomNames(RoomGroups);
        }

        public string Name { get; set; }

        public ICommand PlaceCommand { get; }

        public BindingList<RoomGroupViewModel> RoomGroups { get; }

        public ObservableCollection<string> RoomNames {
            get {
                return roomNames;                
            }
            set {
                roomNames = value;
                OnPropertyChanged();
            }
        }

        public List<RoomTagTypeModel> TagFamilies { get; }

        protected abstract BindingList<RoomGroupViewModel> GetGroupViewModels();

        public GroupPlacementWay PlacementWayByGroups {
            get { return placementWayByGroups; }
            set {
                if (placementWayByGroups == value) return;

                placementWayByGroups = value;
                OnPropertyChanged("PlacementWayByGroups");
                OnPropertyChanged("IsEveryRoom");
                OnPropertyChanged("IsOneRoomPerGroup");
            }
        }

        public bool IsEveryRoom {
            get { return PlacementWayByGroups == GroupPlacementWay.EveryRoom; }
            set { PlacementWayByGroups = value ? GroupPlacementWay.EveryRoom : PlacementWayByGroups; }
        }

        public bool IsOneRoomPerGroup {
            get { return PlacementWayByGroups == GroupPlacementWay.OneRoomPerGroup; }
            set { PlacementWayByGroups = value ? GroupPlacementWay.OneRoomPerGroup : PlacementWayByGroups; }
        }

        public RoomTagTypeModel SelectedTagType {
            get => _selectedTagType;
            set => this.RaiseAndSetIfChanged(ref _selectedTagType, value);
        }

        private void PlaceTags(object p) {
            _revitRepository.PlaceTagsCommand(RoomGroups, 
                                              SelectedTagType.TagId,
                                              PlacementWayByGroups);
        }

        private bool CanPlaceTags(object p) {
            return true;
        }
    }

}
