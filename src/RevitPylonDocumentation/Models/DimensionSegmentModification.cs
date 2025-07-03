using Autodesk.Revit.DB;

namespace RevitPylonDocumentation.Models;
internal class DimensionSegmentModification {
    public DimensionSegmentModification(bool modificationNeeded) {
        ModificationNeeded = modificationNeeded;
    }

    public DimensionSegmentModification(bool modificationNeeded, string prefix, XYZ textOffset) {
        ModificationNeeded = modificationNeeded;
        Prefix = prefix;
        TextOffset = textOffset;
    }

    public bool ModificationNeeded { get; }
    public string Prefix { get; }
    public XYZ TextOffset { get; }
}
