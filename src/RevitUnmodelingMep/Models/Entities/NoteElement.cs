using System.Collections.Generic;

namespace RevitUnmodelingMep.Models.Entities;

internal sealed class NoteElement {
    public double SumArea_m2 { get; set; }
    public double SumLength_mm { get; set; }
    public double SumLength_m { get; set; }
    public double Count { get; set; }
    public double SumAreaWithStock_m2 { get; set; }
    public double SumLengthWithStock_mm { get; set; }
    public double SumLengthWithStock_m { get; set; }

    public static IReadOnlyList<string> GetTokenNames() {
        return new[] {
            nameof(SumArea_m2),
            nameof(SumAreaWithStock_m2),
            nameof(SumLength_mm),
            nameof(SumLengthWithStock_mm),
            nameof(SumLength_m),
            nameof(SumLengthWithStock_m),
            nameof(Count)
        };
    }
}
