using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitRoundingOfAreas.Models;

namespace RevitRoundingOfAreas.ViewModels.Warnings;

internal class WarningElementViewModel : BaseViewModel {
    private ElementId _elementId;
    private string _name;
    private string _levelName;

    public WarningElementViewModel(SpatialModel spatialModel) {
        Name = spatialModel.SpatialElement.Name;
        LevelName = spatialModel.LevelName;
        ElementId = spatialModel.SpatialElement.Id;
    }

    public ICommand ShowElementCommand { get; set; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
    public string LevelName {
        get => _levelName;
        set => RaiseAndSetIfChanged(ref _levelName, value);
    }
    public ElementId ElementId {
        get => _elementId;
        set => RaiseAndSetIfChanged(ref _elementId, value);
    }
}
