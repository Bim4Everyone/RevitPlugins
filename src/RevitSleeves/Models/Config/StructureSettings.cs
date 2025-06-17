using Autodesk.Revit.DB;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterModel;

namespace RevitSleeves.Models.Config;
internal class StructureSettings {
    [JsonConstructor]
    public StructureSettings() { }


    public BuiltInCategory Category { get; set; }

    public Set FilterSet { get; set; }

    public bool IsEnabled { get; set; }
}
