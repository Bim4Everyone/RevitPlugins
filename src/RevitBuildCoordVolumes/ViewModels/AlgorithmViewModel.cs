using dosymep.WPF.ViewModels;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.ViewModels;
internal class AlgorithmViewModel : BaseViewModel {
    private string _name;

    public AlgorithmType AlgorithmType { get; set; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
