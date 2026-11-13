using Autodesk.Revit.DB;

namespace RevitPylonDocumentation.Models.UserSettings;
internal class UserLegendsAndAnnotationsSettings {
    public string LegendXOffset { get; set; }
    public string LegendYOffset { get; set; }
    public string LegendName { get; set; }
    public View SelectedLegend { get; set; }
}
