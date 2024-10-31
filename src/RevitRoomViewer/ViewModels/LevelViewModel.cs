using System.Collections.Generic;
using System.Collections.ObjectModel;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitRoomViewer.Models;

namespace RevitRoomViewer.ViewModels {
    internal class LevelViewModel : BaseViewModel {

        private readonly Level _level;
        private readonly ObservableCollection<RoomElement> _rooms;

        public LevelViewModel(string name, Level level, List<RoomElement> rooms) {
            Name = name;
            _level = level;
            _rooms = new ObservableCollection<RoomElement>(rooms);
        }

        public Level Element { get => _level; }
        public ObservableCollection<RoomElement> Rooms { get => _rooms; }
        public string Name { get; set; }
    }
}
