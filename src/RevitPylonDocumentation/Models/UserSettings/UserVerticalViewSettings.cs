using Autodesk.Revit.DB;

namespace RevitPylonDocumentation.Models.UserSettings;

internal class UserVerticalViewSettings {
    public string GeneralViewPrefix { get; set; }
    public string GeneralViewSuffix { get; set; }
    public string GeneralViewPerpendicularPrefix { get; set; }
    public string GeneralViewPerpendicularSuffix { get; set; }
    public string GeneralRebarViewPrefix { get; set; }
    public string GeneralRebarViewSuffix { get; set; }
    public string GeneralRebarViewPerpendicularPrefix { get; set; }
    public string GeneralRebarViewPerpendicularSuffix { get; set; }
    public string GeneralViewTemplateName { get; set; }
    public string GeneralRebarViewTemplateName { get; set; }
    public string GeneralViewXOffset { get; set; }
    public string GeneralViewYTopOffset { get; set; }
    public string GeneralViewYBottomOffset { get; set; }
    public string GeneralViewPerpXOffset { get; set; }
    public string GeneralViewPerpYTopOffset { get; set; }
    public string GeneralViewPerpYBottomOffset { get; set; }
    public string GeneralViewFamilyTypeName { get; set; }
    public ViewFamilyType SelectedGeneralViewFamilyType { get; set; }
}
