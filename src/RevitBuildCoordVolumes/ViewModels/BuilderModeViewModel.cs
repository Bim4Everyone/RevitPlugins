using dosymep.WPF.ViewModels;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.ViewModels;

internal class BuilderModeViewModel : BaseViewModel {
    private string _name;

    public BuilderMode BuilderMode { get; set; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
