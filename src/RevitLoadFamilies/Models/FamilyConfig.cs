using System.Collections.Generic;

namespace RevitLoadFamilies.Models;
public class FamilyConfig {
    public string Name { get; set; }
    public List<string> FamilyPaths { get; set; }

    public FamilyConfig() {
        FamilyPaths = new List<string>();
    }

    public FamilyConfig(string name) : this() {
        Name = name;
    }
}
