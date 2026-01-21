using Autodesk.Revit.DB;

namespace RevitPylonDocumentation.Models.UserSettings;
internal class UserProjectSettings {
    public string DispatcherGroupingFirst { get; set; }
    public string DispatcherGroupingSecond { get; set; }
    public string DimensionTypeName { get; set; }
    public string SpotDimensionTypeName { get; set; }
    public string SkeletonTagTypeName { get; set; }
    public string RebarTagTypeWithSerifName { get; set; }
    public string RebarTagTypeWithStepName { get; set; }
    public string RebarTagTypeWithCommentName { get; set; }
    public string UniversalTagTypeName { get; set; }
    public string BreakLineTypeName { get; set; }
    public string ConcretingJointTypeName { get; set; }
    public bool DimensionGrouping { get; set; }
    public DimensionType SelectedDimensionType { get; set; }
    public FamilySymbol SelectedSkeletonTagType { get; set; }
    public FamilySymbol SelectedRebarTagTypeWithSerif { get; set; }
    public FamilySymbol SelectedRebarTagTypeWithStep { get; set; }
    public FamilySymbol SelectedRebarTagTypeWithComment { get; set; }
    public FamilySymbol SelectedUniversalTagType { get; set; }
    public FamilySymbol SelectedBreakLineType { get; set; }
    public FamilySymbol SelectedConcretingJointType { get; set; }
    public SpotDimensionType SelectedSpotDimensionType { get; set; }
}
