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

    public SelectedOnViewSelection(IDocumentProvider documentProvider) {
        _documentProvider = documentProvider;
    }

    public IEnumerable<SpotDimension> GetElements() {
        return _documentProvider.GetUIDocument()
            .GetSelectedElements()
            .OfType<SpotDimension>()
            .Where(item => item.FilterSpotDimensions(RevitRepository.FilterSpotName));
    }
}
