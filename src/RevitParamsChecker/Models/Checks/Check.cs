using System.Collections.Generic;

namespace RevitParamsChecker.Models.Checks;

internal class Check {
    public Check() {
    }

    public string Name { get; set; }

    public HashSet<string> Filters { get; set; } = [];

    public HashSet<string> Files { get; set; } = [];

    public HashSet<string> Rules { get; set; } = [];

    public Check Copy() {
        return new Check() {
            Name = Name,
            Filters = [..Filters],
            Files = [..Files],
            Rules = [..Rules]
        };
    }
}
