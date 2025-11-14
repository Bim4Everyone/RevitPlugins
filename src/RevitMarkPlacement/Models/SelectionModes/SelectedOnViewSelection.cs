using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

using RevitMarkPlacement.Extensions;

namespace RevitMarkPlacement.Models.SelectionModes;

internal sealed class SelectedOnViewSelection : ISpotDimensionSelection {
    private readonly IDocumentProvider _documentProvider;
    private readonly SystemPluginConfig _systemPluginConfig;

    public SelectedOnViewSelection(IDocumentProvider documentProvider,SystemPluginConfig systemPluginConfig) {
        _documentProvider = documentProvider;
        _systemPluginConfig = systemPluginConfig;
    }
    
    public Selections Selections => Selections.SelectedOnViewSelection;

    public IEnumerable<SpotDimension> GetElements() {
        return _documentProvider.GetUIDocument()
            .GetSelectedElements()
            .OfType<SpotDimension>()
            .Where(item => item.FilterSpotDimensions(_systemPluginConfig.FilterSpotName));
    }
}
