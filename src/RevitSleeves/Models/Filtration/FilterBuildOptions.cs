using Bim4Everyone.RevitFiltration;

namespace RevitSleeves.Models.Filtration;

internal static class FilterBuildOptions {
    public static Options Create() {
        return new Options() { Tolerance = 0.001, FilterByType = false };
    }

    public static Options CreateInverted() {
        return new Options() { Tolerance = 0.001, FilterByType = false, Inverted = true };
    }
}
