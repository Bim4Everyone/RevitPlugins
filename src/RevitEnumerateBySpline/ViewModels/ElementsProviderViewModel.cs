using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models.Interfaces;

namespace RevitEnumerateBySpline.ViewModels;

internal class ElementsProviderViewModel : BaseViewModel{
    private string _name = string.Empty;

    public IElementsProvider? ElementsProvider { get; set; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
