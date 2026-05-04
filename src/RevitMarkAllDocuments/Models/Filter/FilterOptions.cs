using Bim4Everyone.RevitFiltration;

namespace RevitMarkAllDocuments.Models;

internal class FilterOptions : IOptions {
    public double Tolerance { get; set; }
}
