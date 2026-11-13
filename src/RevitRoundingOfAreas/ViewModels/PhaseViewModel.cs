using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitRoundingOfAreas.ViewModels;

internal class PhaseViewModel : BaseViewModel {
    private string _name;

    public ElementId ElementId { get; set; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
