using dosymep.WPF.ViewModels;

namespace RevitRoundingOfAreas.ViewModels;
internal class DigitViewModel : BaseViewModel {
    private string _name;

    public int DigitCount { get; set; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
