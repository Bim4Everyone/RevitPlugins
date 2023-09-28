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
        private string _selectedRoomName;
        private GroupPlacementWay placementWayByGroups = GroupPlacementWay.EveryRoom;
        private ObservableCollection<string> roomNames;
        private string _errorText;

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
            if(IsOneRoomPerGroupByName)
                OnPropertyChanged("RoomNames");
        }

        public string Name { get; set; }

        public ICommand PlaceCommand { get; }

        public BindingList<RoomGroupViewModel> RoomGroups { get; }

        public ObservableCollection<string> RoomNames {
            get => _revitRepository.GetRoomNames(RoomGroups);  
            set { }
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
                OnPropertyChanged("OneRoomPerGroupRandom");
                OnPropertyChanged("OneRoomPerGroupByName");
                OnPropertyChanged("RoomNames");
            }
        }

        public bool IsEveryRoom {
            get { return PlacementWayByGroups == GroupPlacementWay.EveryRoom; }
            set { PlacementWayByGroups = value ? GroupPlacementWay.EveryRoom : PlacementWayByGroups; }
        }

        public bool IsOneRoomPerGroupRandom {
            get { return PlacementWayByGroups == GroupPlacementWay.OneRoomPerGroupRandom; }
            set { PlacementWayByGroups = value ? GroupPlacementWay.OneRoomPerGroupRandom : PlacementWayByGroups; }
        }

        public bool IsOneRoomPerGroupByName {
            get { return PlacementWayByGroups == GroupPlacementWay.OneRoomPerGroupByName; }
            set { PlacementWayByGroups = value ? GroupPlacementWay.OneRoomPerGroupByName : PlacementWayByGroups; }
        }

        public RoomTagTypeModel SelectedTagType {
            get => _selectedTagType;
            set => this.RaiseAndSetIfChanged(ref _selectedTagType, value);
        }

        public string SelectedRoomName {
            get => _selectedRoomName;
            set => this.RaiseAndSetIfChanged(ref _selectedRoomName, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        private void PlaceTags(object p) {
            _revitRepository.PlaceTagsCommand(RoomGroups, 
                                              SelectedTagType.TagId,
                                              PlacementWayByGroups,
                                              SelectedRoomName);
        }

        private bool CanPlaceTags(object p) {
            if(RoomGroups.Where(x => x.IsChecked).Count() == 0) {
                ErrorText = "Группы не выбраны";
                return false; 
            }
            if(IsOneRoomPerGroupByName && RoomNames.Count == 0) {
                ErrorText = "Для выбранных групп нет общих помещений";
                return false;
            }
            if(IsOneRoomPerGroupByName && SelectedRoomName == null) {
                ErrorText = "Имя помещения не выбрано";
                return false;
            }

            ErrorText = "";
            return true;
        }
    }

}
