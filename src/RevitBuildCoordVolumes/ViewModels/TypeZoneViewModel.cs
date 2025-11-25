using dosymep.WPF.ViewModels;

namespace RevitBuildCoordVolumes.ViewModels;
internal class TypeZoneViewModel : BaseViewModel {

    private string _name;

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
