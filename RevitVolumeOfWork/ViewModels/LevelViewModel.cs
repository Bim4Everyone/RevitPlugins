using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitVolumeOfWork.Models;

namespace RevitVolumeOfWork.ViewModels {
    internal class LevelViewModel : BaseViewModel {

        readonly Level _level;
        readonly IEnumerable<RoomElement> _rooms;

        private bool _isSelected;
                
        public LevelViewModel(string name, Level level, IEnumerable<RoomElement> rooms) {
            _level = level;
            _rooms = rooms;
            Name = name;
        }

        public string Name { get; set; }

        public Level Element { get => _level;  }

        public bool IsSelected {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public IEnumerable<RoomElement> Rooms { get => _rooms;  }
    }
}
