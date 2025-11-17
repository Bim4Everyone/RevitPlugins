using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models.SelectionModes;

internal sealed class DoubleGlobalParamSelection : IGlobalParamSelection {
    private readonly IDocumentProvider _documentProvider;

    public DoubleGlobalParamSelection(IDocumentProvider documentProvider) {
        _documentProvider = documentProvider;
    }

    public IEnumerable<GlobalParameter> GetElements() {
        var document = _documentProvider.GetDocument();
        return GlobalParametersManager
            .GetGlobalParametersOrdered(document)
            .Select(id => document.GetElement(id))
            .Cast<GlobalParameter>()
            .Where(p => p.GetValue() is DoubleParameterValue);
    }
}
