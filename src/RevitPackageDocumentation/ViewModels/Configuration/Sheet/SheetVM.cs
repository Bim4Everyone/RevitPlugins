using System.Collections.Generic;

using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet;
internal class SheetVM {
    public string Name { get; set; }
    public SheetPropsVM Properties { get; set; }
    public List<SheetComponentVM> SheetComponents { get; set; } = [];
}
