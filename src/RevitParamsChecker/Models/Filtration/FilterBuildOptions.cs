using Bim4Everyone.RevitFiltration;

namespace RevitParamsChecker.Models.Filtration;

internal class FilterBuildOptions : IOptions {
    public double Tolerance { get; set; } = 0.001;
}
