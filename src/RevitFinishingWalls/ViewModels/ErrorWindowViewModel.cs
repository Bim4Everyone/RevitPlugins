using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitFinishingWalls.ViewModels {
    internal class ErrorWindowViewModel : BaseViewModel {
        public ErrorWindowViewModel() {
            Rooms = new ObservableCollection<RoomErrorsViewModel>();
            SelectedRoomErrors = new ObservableCollection<ErrorViewModel>();

            SelectErrorCommand = RelayCommand.Create<ErrorViewModel>(SelectAndShowError);
        }


        public ICommand SelectErrorCommand { get; }


        public string Title => "Ошибки создания отделки стен";


        public ObservableCollection<RoomErrorsViewModel> Rooms { get; }

        private RoomErrorsViewModel _selectedRoom;
        public RoomErrorsViewModel SelectedRoom {
            get => _selectedRoom;
            set {
                RaiseAndSetIfChanged(ref _selectedRoom, value);
                SelectedRoomErrors.Clear();
                foreach(ErrorViewModel error in _selectedRoom.Errors) {
                    SelectedRoomErrors.Add(error);
                }
            }
        }

        public ObservableCollection<ErrorViewModel> SelectedRoomErrors { get; }


        public void LoadRooms(ICollection<RoomErrorsViewModel> rooms) {
            if(rooms is null) { throw new ArgumentNullException(nameof(rooms)); }
            if(rooms.Count == 0) { throw new ArgumentOutOfRangeException(nameof(rooms)); }
            if(rooms.Where(room => room.Errors.Count == 0).Any()) { throw new ArgumentException(nameof(rooms)); }

            Rooms.Clear();
            SelectedRoomErrors.Clear();
            SelectedRoom = default;
            foreach(RoomErrorsViewModel room in rooms) {
                Rooms.Add(room);
            }
            SelectedRoom = Rooms.First();
        }

        public void SelectAndShowError(ErrorViewModel error) {

        }
    }
}
