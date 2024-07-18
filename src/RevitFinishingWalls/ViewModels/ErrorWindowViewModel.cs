using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitFinishingWalls.Models;

namespace RevitFinishingWalls.ViewModels {
    internal class ErrorWindowViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly IMessageBoxService _messageBoxService;

        public ErrorWindowViewModel(RevitRepository revitRepository, IMessageBoxService messageBoxService) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));

            Rooms = new ObservableCollection<RoomErrorsViewModel>();
            SelectedRoomErrors = new ObservableCollection<ErrorViewModel>();

            SelectErrorCommand = RelayCommand.Create<IElementsContainer>(SelectAndShowError, CanSelectAndShowError);
        }

        public IMessageBoxService MessageBoxService => _messageBoxService;

        public ICommand SelectErrorCommand { get; }


        public string Title => "Ошибки создания отделки стен";


        public ObservableCollection<RoomErrorsViewModel> Rooms { get; }

        private RoomErrorsViewModel _selectedRoom;
        public RoomErrorsViewModel SelectedRoom {
            get => _selectedRoom;
            set {
                RaiseAndSetIfChanged(ref _selectedRoom, value);
                SelectedRoomErrors.Clear();
                if(_selectedRoom != null) {
                    foreach(ErrorViewModel error in _selectedRoom.Errors) {
                        SelectedRoomErrors.Add(error);
                    }
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

        private void SelectAndShowError(IElementsContainer error) {
            try {
                _revitRepository.ShowElementsOnActiveView(error.DependentElements);
            } catch(InvalidOperationException ex) {
                _messageBoxService.Show(
                    ex.Message,
                    "BIM",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
        private bool CanSelectAndShowError(IElementsContainer error) {
            return error != null;
        }
    }
}
