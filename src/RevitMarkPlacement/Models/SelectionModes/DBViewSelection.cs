using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitMarkPlacement.Extensions;

namespace RevitMarkPlacement.Models.SelectionModes;

internal sealed class DBViewSelection : ISpotDimensionSelection {
    private readonly IDocumentProvider _documentProvider;
    private readonly SystemPluginConfig _systemPluginConfig;

    public DBViewSelection(IDocumentProvider documentProvider, SystemPluginConfig systemPluginConfig) {
        _documentProvider = documentProvider;
        _systemPluginConfig = systemPluginConfig;
    }
    
    public Selections Selections => Selections.DBViewSelection;

    public IEnumerable<SpotDimension> GetElements() {
        var document = _documentProvider.GetDocument();
        return new FilteredElementCollector(document, document.ActiveView.Id)
            .OfClass(typeof(SpotDimension))
            .OfType<SpotDimension>()
            .Where(item => item.FilterSpotDimensions(_systemPluginConfig.FilterSpotName));
    }
}
