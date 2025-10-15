using Autodesk.Revit.DB;

namespace RevitPylonDocumentation.Models.PluginOptions;
internal class DimensionSegmentOption {
    public DimensionSegmentOption(bool modificationNeeded) {
        ModificationNeeded = modificationNeeded;
    }

    public DimensionSegmentOption(bool modificationNeeded, string prefix, XYZ textOffset) {
        ModificationNeeded = modificationNeeded;
        Prefix = prefix;
        TextOffset = textOffset;
    }

    public bool ModificationNeeded { get; }
    public string Prefix { get; }
    public XYZ TextOffset { get; }
}
