using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitVolumeModifier.Models;

internal class DirectShapeOrInPlaceFilter : ISelectionFilter {
    private readonly Document _doc;

    public DirectShapeOrInPlaceFilter(Document doc) {
        _doc = doc;
    }

    public bool AllowElement(Element elem) {
        return elem is DirectShape || (elem is FamilyInstance fi
            ? fi.Symbol?.Family?.IsInPlace ?? false
            : _doc.GetElement(elem.GetTypeId()) is FamilySymbol fs && (fs.Family?.IsInPlace ?? false));
    }

    public bool AllowReference(Reference reference, XYZ position) {
        return true;
    }
}
