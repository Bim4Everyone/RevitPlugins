using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitVolumeOfWork.Models;

namespace RevitVolumeOfWork.ViewModels; 
internal class LevelViewModel : BaseViewModel {
    private bool _isSelected;

    public LevelViewModel(string name, Level level, IEnumerable<RoomElement> rooms) {
        Element = level;
        Rooms = rooms;
        Name = name;
    }

    public string Name { get; set; }

    public Level Element { get; }

    public bool IsSelected {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public IEnumerable<RoomElement> Rooms { get; }
}
