using Autodesk.Revit.DB;

namespace RevitDeclarations.Models;
internal class CommercialSettings : DeclarationSettings {
    public Parameter BuildingNumberParam { get; set; }
    public Parameter ConstrWorksNumberParam { get; set; }
    public Parameter RoomsHeightParam { get; set; }
    public Parameter ParkingSpaceClass { get; set; }
    public Parameter ParkingInfo { get; set; }

    public Parameter GroupNameParam { get; set; }
    public bool AddPrefixToNumber { get; set; }
}
