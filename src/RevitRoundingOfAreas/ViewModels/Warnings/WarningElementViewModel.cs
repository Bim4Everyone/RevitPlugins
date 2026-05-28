using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitRoundingOfAreas.Models;

namespace RevitRoundingOfAreas.ViewModels.Warnings;

internal class WarningElementViewModel : BaseViewModel {
    private readonly SpatialModel _spatialModel;
    private ElementId _elementId;
    private string _name;
    private string _levelName;

    public WarningElementViewModel(SpatialModel spatialModel) {
        _spatialModel = spatialModel;
        ElementId = _spatialModel.SpatialElement.Id;
        Name = _spatialModel.SpatialElement.Name;
        LevelName = _spatialModel.LevelName;
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
}
