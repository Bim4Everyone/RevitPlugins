using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitMarkPlacement.Extensions;

namespace RevitMarkPlacement.Models.SelectionModes;

internal sealed class DBViewSelection : ISpotDimensionSelection {
    private readonly IDocumentProvider _documentProvider;

    public DBViewSelection(IDocumentProvider documentProvider) {
        _documentProvider = documentProvider;
    }
    
    public Selections Selections => Selections.SelectedOnViewSelection;

    public IEnumerable<SpotDimension> GetElements() {
        var document = _documentProvider.GetDocument();
        return new FilteredElementCollector(document, document.ActiveView.Id)
            .OfClass(typeof(SpotDimension))
            .OfType<SpotDimension>()
            .Where(item => item.FilterSpotDimensions(RevitRepository.FilterSpotName));
    }
}
