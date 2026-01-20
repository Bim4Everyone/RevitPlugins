using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitBuildCoordVolumes.ViewModels;
internal class LevelViewModel : BaseViewModel {
    private string _name;

    public Level Level { get; set; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
