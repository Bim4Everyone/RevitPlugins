using Autodesk.Revit.DB;

namespace RevitSleeves.Models.Config;
internal class WallSettings : StructureSettings {
    public WallSettings() : base() { }

    public override BuiltInCategory Category => BuiltInCategory.OST_Walls;
}
