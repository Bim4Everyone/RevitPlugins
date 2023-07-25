using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.ViewModels;

using RevitVolumeOfWork.Models;

namespace RevitVolumeOfWork.ViewModels {
    internal class LevelViewModel : BaseViewModel {

        Level _level;
        private bool _isSelected;

        IEnumerable<RoomElement> _rooms;
        
        public LevelViewModel(string name, Level level, RevitRepository revitRepository,
            IEnumerable<RoomElement> rooms) {

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

        public ObservableCollection<RoomElement> Rooms { get => (ObservableCollection<RoomElement>) _rooms;  }
    }
}
