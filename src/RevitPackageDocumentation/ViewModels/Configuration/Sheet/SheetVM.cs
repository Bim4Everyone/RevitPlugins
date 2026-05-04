using System.Collections.Generic;

using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet;
internal class SheetVM {

    public SheetPropsVM SheetProperties { get; set; }
    public List<SheetComponentVM> SheetComponents { get; set; }
}
