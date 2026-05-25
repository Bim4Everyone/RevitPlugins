using dosymep.Bim4Everyone;
using dosymep.WPF.ViewModels;

namespace RevitRoundingOfAreas.ViewModels;
internal class ParamViewModel : BaseViewModel {
    private string _name;

    public RevitParam RevitParam { get; set; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
