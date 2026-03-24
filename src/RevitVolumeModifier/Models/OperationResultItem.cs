using Autodesk.Revit.DB;

namespace RevitVolumeModifier.Models;
internal class OperationResultItem {
    public ElementId SourceId { get; set; }
    public DirectShapeObject Shape { get; set; }
}
