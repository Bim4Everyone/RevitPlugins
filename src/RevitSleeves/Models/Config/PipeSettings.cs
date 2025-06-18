using Autodesk.Revit.DB;

namespace RevitSleeves.Models.Config;
internal class PipeSettings : MepCategorySettings {
    public PipeSettings() : base() { }

    public override BuiltInCategory Category => BuiltInCategory.OST_PipeCurves;
}
