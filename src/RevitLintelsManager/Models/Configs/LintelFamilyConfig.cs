using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitLintelsManager.Models.Configs;

public class LintelFamilyConfig {
    public RevitFamily RevitLintelFamily { get; set; }
    public RevitParam LintelWidth { get; set; }
    public RevitParam LintelThickness { get; set; }
    public RevitParam LintelRightOffset { get; set; }
    public RevitParam LintelLeftOffset { get; set; }
    public RevitParam LintelRightCorner { get; set; }
    public RevitParam LintelLeftCorner { get; set; }
    public RevitParam LintelRightWelding { get; set; }
    public RevitParam LintelLeftWelding { get; set; }
}
