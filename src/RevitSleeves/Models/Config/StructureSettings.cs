using Autodesk.Revit.DB;

using pyRevitLabs.Json;

namespace RevitSleeves.Models.Config;
internal abstract class StructureSettings {
    protected StructureSettings() { }


    [JsonIgnore]
    public abstract BuiltInCategory Category { get; }

    /// <summary>
    /// Сериализованный контекст фильтра (<see cref="Bim4Everyone.RevitFiltration.Controls.ILogicalFilterContext"/>)
    /// </summary>
    public string FilterContext { get; set; }

    public bool IsEnabled { get; set; } = true;
}
