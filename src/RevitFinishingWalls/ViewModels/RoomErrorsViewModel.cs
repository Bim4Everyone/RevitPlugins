using System.Collections.Generic;
using System.Collections.ObjectModel;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.ViewModels;

namespace RevitFinishingWalls.ViewModels {
    /// <summary>
    /// Модель представления ошибок создания отделки стен в конкретном помещении
    /// </summary>
    internal class RoomErrorsViewModel : BaseViewModel, IElementsContainer {
        private readonly Room _room;

        public RoomErrorsViewModel(Room room) {
            _room = room ?? throw new System.ArgumentNullException(nameof(room));

            Errors = new List<ErrorViewModel>();
            DependentElements = new ReadOnlyCollection<ElementId>(new ElementId[] { _room.Id });
        }


        public string Name => _room.Name;

        public string LevelName => _room.Level.Name;

        public string RoomNumber => _room.Number;

        public string RoomId => _room.Id.ToString();




        public ICollection<ErrorViewModel> Errors { get; }

        public IReadOnlyCollection<ElementId> DependentElements { get; }
    }
}
