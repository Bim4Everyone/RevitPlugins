using Bim4Everyone.RevitFiltration;

namespace RevitParamsChecker.Models.Revit;

internal class FilterBuildOptions : IOptions {
    public double Tolerance { get; set; } = 0.001;
}
