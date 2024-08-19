using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.ViewModels;

namespace RevitFinishing.ViewModels
{
    internal class RoomGroupViewModel : BaseViewModel {
        private readonly string _name;
        private readonly IReadOnlyCollection<Room> _rooms;

        private bool _isChecked;

        public RoomGroupViewModel(string name, IEnumerable<Room> elements) {
            _rooms = elements.ToList();
            _name = name;
        }

        public IReadOnlyCollection<Room> Rooms => _rooms;

        public string Name => _name;

        public bool IsChecked {
            get => _isChecked;
            set => RaiseAndSetIfChanged(ref _isChecked, value);
        }
    }
}
