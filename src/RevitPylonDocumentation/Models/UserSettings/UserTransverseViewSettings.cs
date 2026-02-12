using Autodesk.Revit.DB;

namespace RevitPylonDocumentation.Models.UserSettings;

internal class UserTransverseViewSettings {
    public string TransverseViewDepth { get; set; }
    public string TransverseViewFirstPrefix { get; set; }
    public string TransverseViewFirstSuffix { get; set; }
    public string TransverseViewFirstElevation { get; set; }
    public string TransverseViewSecondPrefix { get; set; }
    public string TransverseViewSecondSuffix { get; set; }
    public string TransverseViewSecondElevation { get; set; }
    public string TransverseViewThirdPrefix { get; set; }
    public string TransverseViewThirdSuffix { get; set; }
    public string TransverseViewThirdElevation { get; set; }
    public string TransverseRebarViewDepth { get; set; }
    public string TransverseRebarViewFirstPrefix { get; set; }
    public string TransverseRebarViewFirstSuffix { get; set; }
    public string TransverseRebarViewSecondPrefix { get; set; }
    public string TransverseRebarViewSecondSuffix { get; set; }
    public string TransverseRebarViewThirdPrefix { get; set; }
    public string TransverseRebarViewThirdSuffix { get; set; }
    public string TransverseViewTemplateName { get; set; }
    public string TransverseRebarViewTemplateName { get; set; }
    public string TransverseViewXOffset { get; set; }
    public string TransverseViewYOffset { get; set; }
    public string TransverseViewFamilyTypeName { get; set; }
    public ViewFamilyType SelectedTransverseViewFamilyType { get; set; }
    public View SelectedTransverseViewTemplate { get; set; }
    public View SelectedTransverseRebarViewTemplate { get; set; }
}
