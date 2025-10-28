using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models.UnitProviders;

internal sealed class UnitProvider : IUnitProvider {
    private readonly IDocumentProvider _documentProvider;

    public UnitProvider(IDocumentProvider documentProvider) {
        _documentProvider = documentProvider;
    }
    
    public Units GetUnits() {
        return _documentProvider.GetDocument().GetUnits();
    } 
}
