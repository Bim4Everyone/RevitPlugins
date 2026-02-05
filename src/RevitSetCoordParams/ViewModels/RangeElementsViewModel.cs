using dosymep.WPF.ViewModels;

using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.ViewModels;
internal class RangeElementsViewModel : BaseViewModel {

    private string _name;

    public IElementsProvider ElementsProvider { get; set; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
