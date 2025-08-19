using Autodesk.Revit.DB;

namespace RevitSleeves.Models.Config;
internal class FloorSettings : StructureSettings {
    public FloorSettings() : base() { }

    public override BuiltInCategory Category => BuiltInCategory.OST_Floors;
}
