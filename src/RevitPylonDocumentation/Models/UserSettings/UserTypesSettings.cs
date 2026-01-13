using Autodesk.Revit.DB;

namespace RevitPylonDocumentation.Models.UserSettings;
internal class UserTypesSettings {
    //public ViewFamilyType SelectedViewFamilyType { get; set; }
    public DimensionType SelectedDimensionType { get; set; }
    public FamilySymbol SelectedSkeletonTagType { get; set; }
    public FamilySymbol SelectedRebarTagTypeWithSerif { get; set; }
    public FamilySymbol SelectedRebarTagTypeWithStep { get; set; }
    public FamilySymbol SelectedRebarTagTypeWithComment { get; set; }
    public FamilySymbol SelectedUniversalTagType { get; set; }
    public FamilySymbol SelectedBreakLineType { get; set; }
    public FamilySymbol SelectedConcretingJointType { get; set; }
    public SpotDimensionType SelectedSpotDimensionType { get; set; }
    //public View SelectedGeneralViewTemplate { get; set; }
    //public View SelectedGeneralRebarViewTemplate { get; set; }
    //public View SelectedTransverseViewTemplate { get; set; }
    //public View SelectedTransverseRebarViewTemplate { get; set; }
}
