using Autodesk.Revit.DB;

using pyRevitLabs.Json;

namespace RevitSleeves.Models.Config;
internal abstract class MepCategorySettings {
    protected MepCategorySettings() { }


    [JsonIgnore]
    public abstract BuiltInCategory Category { get; }

    /// <summary>
    /// Сериализованный контекст фильтра (<see cref="Bim4Everyone.RevitFiltration.Controls.ILogicalFilterContext"/>)
    /// </summary>
    public string MepFilterContext { get; set; }

    public Offset[] Offsets { get; set; } = [];

    public DiameterRange[] DiameterRanges { get; set; } = [];

    public WallSettings WallSettings { get; set; } = new WallSettings();

    public FloorSettings FloorSettings { get; set; } = new FloorSettings();
}
