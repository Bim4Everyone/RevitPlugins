using Autodesk.Revit.DB;

namespace RevitPylonDocumentation.Models.UserSettings;
internal class UserSheetSettings {
    public string SheetPrefix { get; set; }
    public string SheetSuffix { get; set; }
    public string SheetSize { get; set; }
    public string SheetCoefficient { get; set; }
    public string TitleBlockName { get; set; }
    public FamilySymbol SelectedTitleBlock { get; set; }
    public bool CustomTitleBlockIsCheck { get; set; }
    public string CustomSheetSizeValue { get; set; }
    public string CustomSheetCoefficientValue { get; set; }
}
