using System.Collections.Generic;

namespace RevitLoadFamilies.Models;
internal class FamilyConfig {
    public FamilyConfig(string name) {
        Name = name;
    }

    public string Name { get; set; }
    public List<string> FamilyPaths { get; set; }
}
