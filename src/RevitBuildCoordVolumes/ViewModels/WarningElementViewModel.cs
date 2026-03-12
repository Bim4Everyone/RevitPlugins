using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.ViewModels;

internal class WarningElementViewModel : BaseViewModel {
    private readonly SpatialObject _spatialObject;
    private ElementId _elementId;
    private string _name;
    private string _levelName;
    private string _description;

    public WarningElementViewModel(SpatialObject spatialObject) {
        _spatialObject = spatialObject;
        ElementId = _spatialObject.SpatialElement.Id;
        Name = _spatialObject.SpatialElement.Name;
        LevelName = _spatialObject.LevelName;
        Description = _spatialObject.Description;
    }

    public ICommand ShowElementCommand { get; set; }

    public ElementId ElementId {
        get => _elementId;
        set => RaiseAndSetIfChanged(ref _elementId, value);
    }
    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
    public string LevelName {
        get => _levelName;
        set => RaiseAndSetIfChanged(ref _levelName, value);
    }
    public string Description {
        get => _description;
        set => RaiseAndSetIfChanged(ref _description, value);
    }
}
