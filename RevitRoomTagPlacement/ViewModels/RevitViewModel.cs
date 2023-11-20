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

        private GroupPlacementWay _placementWayByGroups;
        private PositionPlacementWay _placementWayByPosition;

        private string _errorText;

        public RevitViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            _placementWayByGroups = GroupPlacementWay.EveryRoom;
            _placementWayByPosition = PositionPlacementWay.CenterCenter;

            RoomGroups = new BindingList<RoomGroupViewModel>(GetGroupViewModels());

            TagFamilies = revitRepository.GetRoomTags();
            if(TagFamilies.Count != 0) SelectedTagType = TagFamilies[0];

            PlaceTagsCommand = RelayCommand.Create(PlaceTags, CanPlaceTags);

            RoomGroups.ListChanged += RoomGroups_ListChanged;
        }

        private void RoomGroups_ListChanged(object sender, ListChangedEventArgs e) {
            if(IsOneRoomPerGroupByName)
                OnPropertyChanged(nameof(RoomNames));
        }

        public string Name { get; set; }

        public ICommand PlaceTagsCommand { get; }

        public BindingList<RoomGroupViewModel> RoomGroups { get; }

        public ObservableCollection<string> RoomNames => _revitRepository.GetRoomNames(RoomGroups);

        public List<RoomTagTypeModel> TagFamilies { get; }

        protected abstract BindingList<RoomGroupViewModel> GetGroupViewModels();

        public GroupPlacementWay PlacementWayByGroups {
            get => _placementWayByGroups;
            set {
                if (_placementWayByGroups == value) return;

                RaiseAndSetIfChanged(ref _placementWayByGroups, value);
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

        public PositionPlacementWay PlacementWayByPosition {
            get => _placementWayByPosition;
            set {
                if(_placementWayByPosition == value) return;

                _placementWayByPosition = value;
            }
        }

        public bool IsPositionLeftTop {
            get { return PlacementWayByPosition == PositionPlacementWay.LeftTop; }
            set { PlacementWayByPosition = value ? PositionPlacementWay.LeftTop : PlacementWayByPosition; }
        }

        public bool IsPositionCenterTop {
            get { return PlacementWayByPosition == PositionPlacementWay.CenterTop; }
            set { PlacementWayByPosition = value ? PositionPlacementWay.CenterTop : PlacementWayByPosition; }
        }

        public bool IsPositionRightTop {
            get { return PlacementWayByPosition == PositionPlacementWay.RightTop; }
            set { PlacementWayByPosition = value ? PositionPlacementWay.RightTop : PlacementWayByPosition; }
        }

        public bool IsPositionLeftCenter {
            get { return PlacementWayByPosition == PositionPlacementWay.LeftCenter; }
            set { PlacementWayByPosition = value ? PositionPlacementWay.LeftCenter : PlacementWayByPosition; }
        }

        public bool IsPositionCenterCenter {
            get { return PlacementWayByPosition == PositionPlacementWay.CenterCenter; }
            set { PlacementWayByPosition = value ? PositionPlacementWay.CenterCenter : PlacementWayByPosition; }
        }

        public bool IsPositionRightCenter {
            get { return PlacementWayByPosition == PositionPlacementWay.RightCenter; }
            set { PlacementWayByPosition = value ? PositionPlacementWay.RightCenter : PlacementWayByPosition; }
        }

        public bool IsPositionLeftBottom {
            get { return PlacementWayByPosition == PositionPlacementWay.LeftBottom; }
            set { PlacementWayByPosition = value ? PositionPlacementWay.LeftBottom : PlacementWayByPosition; }
        }

        public bool IsPositionCenterBottom {
            get { return PlacementWayByPosition == PositionPlacementWay.CenterBottom; }
            set { PlacementWayByPosition = value ? PositionPlacementWay.CenterBottom : PlacementWayByPosition; }
        }

        public bool IsPositionRightBottom {
            get { return PlacementWayByPosition == PositionPlacementWay.RightBottom; }
            set { PlacementWayByPosition = value ? PositionPlacementWay.RightBottom : PlacementWayByPosition; }
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

        private void PlaceTags() {
            _revitRepository.PlaceTagsByPositionAndGroup(RoomGroups, 
                                              SelectedTagType.TagId,
                                              PlacementWayByGroups,
                                              PlacementWayByPosition,
                                              SelectedRoomName);
        }

        private bool CanPlaceTags() {
            if(RoomGroups.Count() == 0) {
                ErrorText = "Помещения отсутствуют/не выбраны";
                return false;
            }
            if(TagFamilies.Count() == 0) {
                ErrorText = "В проекте отсутствуют марки помещений";
                return false;
            }
            if(!RoomGroups.Any(x => x.IsChecked)) {
                ErrorText = "Группы не выбраны";
                return false; 
            }
            if(SelectedTagType == null) {
                ErrorText = "Марка не выбрана";
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
