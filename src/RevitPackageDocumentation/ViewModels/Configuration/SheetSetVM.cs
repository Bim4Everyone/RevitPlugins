using System.Collections.Generic;

using RevitPackageDocumentation.ViewModels.Configuration.Sheet;

namespace RevitPackageDocumentation.ViewModels.Configuration;
internal class SheetSetVM {
    public string Name { get; set; }
    public SheetSetPropsVM Properties { get; set; }
    public List<SheetVM> SheetList { get; set; } = [];
}
