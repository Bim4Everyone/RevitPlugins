using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitSetCoordParams.Models;


namespace RevitSetCoordParams.ViewModels;
internal class WarningElementViewModel : BaseViewModel {
    private readonly RevitElement _revitElement;
    private ElementId _elementId;
    private string _family;
    private string _type;
    private string _level;

    public WarningElementViewModel(RevitElement revitElement) {
        _revitElement = revitElement;
        ID = _revitElement.Element.Id;
        Family = _revitElement.FamilyName;
        Type = _revitElement.TypeName;
        Level = _revitElement.LevelName;
    }

    public ICommand ShowElementCommand { get; set; }

    public ElementId ID {
        get => _elementId;
        set => RaiseAndSetIfChanged(ref _elementId, value);
    }
    public string Family {
        get => _family;
        set => RaiseAndSetIfChanged(ref _family, value);
    }
    public string Type {
        get => _type;
        set => RaiseAndSetIfChanged(ref _type, value);
    }
    public string Level {
        get => _level;
        set => RaiseAndSetIfChanged(ref _level, value);
    }
}
