using System.Collections.Generic;

namespace RevitParamsChecker.Models.Checks;

internal class Check {
    public Check() {
    }

    public string Name { get; set; }

    public HashSet<string> Filters { get; set; }

    public HashSet<string> Files { get; set; }

    public HashSet<string> Rules { get; set; }

    public Check Copy() {
        throw new System.NotImplementedException(); // TODO
    }
}
