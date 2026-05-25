using dosymep.WPF.ViewModels;

using RevitRoundingOfAreas.Models.Interfaces;

namespace RevitRoundingOfAreas.ViewModels;

internal class RangeViewModel : BaseViewModel {

    private string _name;

    public IElementsProvider ElementsProvider { get; set; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
