using dosymep.WPF.ViewModels;

using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.ViewModels;
internal class PositionViewModel : BaseViewModel {

    private string _name;

    public IPositionProvider PositionProvider { get; set; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
