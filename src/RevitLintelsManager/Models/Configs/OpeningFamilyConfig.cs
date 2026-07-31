using System.Collections.Generic;

using dosymep.Bim4Everyone;

namespace RevitLintelsManager.Models.Configs;

public class OpeningFamilyConfig {
    public IEnumerable<RevitFamily> RevitOpeningFamily { get; set; }
    public RevitParam LintelOpeningHeight { get; set; }
    public RevitParam LintelOpeningWidth { get; set; }
}
