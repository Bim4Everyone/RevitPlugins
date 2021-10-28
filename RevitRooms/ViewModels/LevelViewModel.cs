using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.ViewModels;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal class LevelViewModel : BaseViewModel, IComparable<LevelViewModel>, IEquatable<LevelViewModel> {
        private readonly Level _level;

        public LevelViewModel(Level level, RevitRepository revitRepository, IEnumerable<Room> rooms) {
            _level = level;
            Rooms = new ObservableCollection<RoomViewModel>(rooms.Select(item => new RoomViewModel(item, this, revitRepository)));
        }

        public string DisplayData {
            get { return _level.Name; }
        }

        public string Elevation {
            get { return UnitUtils.ConvertFromInternalUnits(_level.Elevation, DisplayUnitType.DUT_METERS) + " " + UnitUtils.GetTypeCatalogString(DisplayUnitType.DUT_METERS); }
        }

        public int RoomsCount {
            get { return Rooms.Count; }
        }

        public ObservableCollection<RoomViewModel> Rooms { get; }

        #region SystemOverrides

        public int CompareTo(LevelViewModel other) {
            return _level.Elevation.CompareTo(other._level.Elevation);
        }

        public override bool Equals(object obj) {
            return Equals(obj as LevelViewModel);
        }

        public bool Equals(LevelViewModel level) {
            return level == null ? false : _level.Id.Equals(level._level.Id);
        }

        public override int GetHashCode() {
            return -2121273300 + _level.Id.GetHashCode();
        }

        #endregion
    }
}
