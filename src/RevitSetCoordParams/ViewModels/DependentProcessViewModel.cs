using dosymep.WPF.ViewModels;

using RevitSetCoordParams.Models.Enums;

namespace RevitSetCoordParams.ViewModels;

internal class DependentProcessViewModel : BaseViewModel {
    private string _name;

    public DependentProcess DependentProcess { get; set; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
