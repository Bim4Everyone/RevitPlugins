using dosymep.Bim4Everyone;

namespace RevitMarkAllDocuments.Models;

internal class FilterableParam {
    public RevitParam RevitParam { get; set; }
    public bool IsTypeParam { get; set; }
}
