using System.Windows.Input;

using Autodesk.Revit.DB;

using RevitSetCoordParams.Models;

namespace RevitSetCoordParams.ViewModels;
internal class WarningElementViewModel {
    private readonly RevitElement _revitElement;

    public WarningElementViewModel(RevitElement revitElement) {
        _revitElement = revitElement;
    }

    public ICommand ShowElementCommand { get; set; }
    public ElementId ID => _revitElement.Element.Id;
    public string Family => _revitElement.FamilyName;
    public string Type => _revitElement.TypeName;
    public string Level => _revitElement.LevelName;
}
