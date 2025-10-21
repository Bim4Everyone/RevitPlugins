using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitMarkPlacement.Extensions;

namespace RevitMarkPlacement.Models.SelectionModes;

internal sealed class DBSelection : ISpotDimensionSelection {
    private readonly IDocumentProvider _documentProvider;

    public DBSelection(IDocumentProvider documentProvider) {
        _documentProvider = documentProvider;
    }

    public Selections Selections => Selections.DBSelection;

    public IEnumerable<SpotDimension> GetElements() {
        return new FilteredElementCollector(_documentProvider.GetDocument())
            .OfClass(typeof(SpotDimension))
            .OfType<SpotDimension>()
            .Where(item => item.FilterSpotDimensions(RevitRepository.FilterSpotName));
    }
}
