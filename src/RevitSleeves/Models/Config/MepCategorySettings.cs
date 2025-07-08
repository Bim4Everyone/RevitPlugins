using Autodesk.Revit.DB;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterModel;

namespace RevitSleeves.Models.Config;
internal abstract class MepCategorySettings {
    protected MepCategorySettings() { }


    [JsonIgnore]
    public abstract BuiltInCategory Category { get; }

    public Set MepFilterSet { get; set; } = new Set();

    public Offset[] Offsets { get; set; } = [];

    public DiameterRange[] DiameterRanges { get; set; } = [];

    public WallSettings WallSettings { get; set; } = new WallSettings();

    public FloorSettings FloorSettings { get; set; } = new FloorSettings();
}
