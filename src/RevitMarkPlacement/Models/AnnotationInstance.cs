using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models;

internal class AnnotationInstance {
    public AnnotationInstance(
        SpotDimension spotDimension,
        AnnotationSymbol annotationSymbol,
        AnnotationSymbolType annotationSymbolType) {
        SpotDimension = spotDimension;
        AnnotationSymbol = annotationSymbol;
        AnnotationSymbolType = annotationSymbolType;
    }

    public SpotDimension SpotDimension { get; }
    public AnnotationSymbol AnnotationSymbol { get; }
    public AnnotationSymbolType AnnotationSymbolType { get; }
}
