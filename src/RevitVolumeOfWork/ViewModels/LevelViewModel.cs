using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitVolumeOfWork.Models;

namespace RevitVolumeOfWork.ViewModels; 
internal class LevelViewModel : BaseViewModel {
    private readonly string _name;
    private readonly Level _element;
    private readonly IEnumerable<RoomElement> _rooms;
    private bool _isChecked;

    public LevelViewModel(string name, Level level, IEnumerable<RoomElement> rooms) {
        _name = name;
        _element = level;
        _rooms = rooms;
    }

    public string Name => _name;
    public Level Element => _element;
    public IEnumerable<RoomElement> Rooms => _rooms;

    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }
}
