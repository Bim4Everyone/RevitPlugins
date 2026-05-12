using dosymep.WPF.ViewModels;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet;
internal class SheetPropsVM : BaseViewModel {
    public string SheetName { get; set; }
    public string SheetSizeValue { get; set; }
    public string SheetCoefficientValue { get; set; }
    public string TitleBlockFamilyName { get; set; }
    public string TitleBlockTypeName { get; set; }
}
