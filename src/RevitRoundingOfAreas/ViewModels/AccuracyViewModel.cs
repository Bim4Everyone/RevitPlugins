using dosymep.WPF.ViewModels;

namespace RevitRoundingOfAreas.ViewModels;
internal class AccuracyViewModel : BaseViewModel {
    private string _name;

    public int Accuracy { get; set; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
