using Autodesk.Revit.DB;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterModel;

namespace RevitSleeves.Models.Config;
internal abstract class StructureSettings {
    protected StructureSettings() { }


    [JsonIgnore]
    public abstract BuiltInCategory Category { get; }

    public Set FilterSet { get; set; } = new Set();

    public bool IsEnabled { get; set; } = true;
}
