using System.Collections.Generic;

using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.ViewModels;

namespace RevitFinishingWalls.ViewModels {
    /// <summary>
    /// Модель представления ошибок создания отделки стен в конкретном помещении
    /// </summary>
    internal class RoomErrorsViewModel : BaseViewModel {
        private readonly Room _room;

        public RoomErrorsViewModel(Room room) {
            _room = room ?? throw new System.ArgumentNullException(nameof(room));

            Errors = new List<ErrorViewModel>();
        }


        public string Name => _room.Name;

        public string LevelName => _room.Level.Name;


        public ICollection<ErrorViewModel> Errors { get; }
    }
}
