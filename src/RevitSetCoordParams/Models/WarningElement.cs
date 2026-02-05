using RevitSetCoordParams.Models.Enums;


namespace RevitSetCoordParams.Models;
internal abstract class WarningElement {
    public WarningType WarningType { get; set; }
    public RevitElement RevitElement { get; set; }
}
