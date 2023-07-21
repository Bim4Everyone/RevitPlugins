using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using RevitVolumeOfWork.Models;

namespace RevitVolumeOfWork.ViewModels {
    internal class LevelViewModel {

        Level _level;
        IEnumerable<RoomElement> _rooms;
        
        public LevelViewModel(string name, Level level, RevitRepository revitRepository,
            IEnumerable<RoomElement> rooms) {

            _level = level;
            _rooms = rooms;

        }

        public Level Element { get => _level;  }

        public ObservableCollection<RoomElement> Rooms { get => (ObservableCollection<RoomElement>) _rooms;  }
    }
}
