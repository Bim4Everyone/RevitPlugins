using System.Collections.Generic;

using RevitPackageDocumentation.ViewModels.Configuration.Sheet;

namespace RevitPackageDocumentation.ViewModels.Configuration;
internal class ConfigurationVM {

    public ConfigurationPropsVM ConfigurationProperties { get; set; }
    public List<SheetVM> SheetList { get; set; }
}
