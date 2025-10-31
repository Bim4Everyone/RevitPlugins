using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using RevitSetCoordParams.Models;

namespace RevitSetCoordParams.ViewModels;
internal class WarningGroupViewModel {
    public IReadOnlyCollection<WarningElement> WarningElements { get; set; }
    public string Caption { get; set; }
    public string Description { get; set; }

    public ObservableCollection<WarningElementViewModel> Warnings => new(GetWarningElementViewModel());

    private IEnumerable<WarningElementViewModel> GetWarningElementViewModel() {
        return WarningElements
            .Select(warningElement => new WarningElementViewModel {
                ID = warningElement.RevitElement.Element.Id.ToString(),
                Family = warningElement.RevitElement.FamilyName,
                Type = warningElement.RevitElement.Element.Name,
                Level = warningElement.RevitElement.LevelName
            });
    }

}
