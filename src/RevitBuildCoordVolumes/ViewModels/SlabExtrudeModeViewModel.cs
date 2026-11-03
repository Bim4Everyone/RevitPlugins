using dosymep.WPF.ViewModels;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.ViewModels;

internal class SlabExtrudeModeViewModel : BaseViewModel {
    private string _name;

    public SlabExtrudeMode SlabExtrudeMode { get; set; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
