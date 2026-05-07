using System.Collections.Generic;

using RevitPackageDocumentation.ViewModels.Configuration.Sheet;

namespace RevitPackageDocumentation.ViewModels.Configuration;
internal class SheetSetVM {
    public string ConfigurationName { get; set; }
    public SheetSetPropsVM ConfigurationProperties { get; set; }
    public List<SheetVM> SheetList { get; set; }
}
