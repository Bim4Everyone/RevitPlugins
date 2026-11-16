using Autodesk.Revit.DB;

namespace RevitDeclarations.Models;
internal class CommercialSettings : DeclarationSettings {
    public const string DefaultParkingKeyword = "зависимое";

    public Parameter BuildingNumberParam { get; set; }
    public Parameter ConstrWorksNumberParam { get; set; }
    public Parameter RoomsHeightParam { get; set; }
    public Parameter ParkingSpaceClass { get; set; }
    public Parameter ParkingInfo { get; set; }
    public Parameter ParkingInfoMainNumberParam { get; set; }
    public Parameter GroupNameMainNumberParam { get; set; }
    public string ParkingInfoKeyword { get; set; } = DefaultParkingKeyword;
    public string GroupNameKeyword { get; set; } = DefaultParkingKeyword;
    public Parameter PositionType { get; set; }

    public Parameter GroupNameParam { get; set; }
    public bool AddPrefixToNumber { get; set; }
    public bool AddHyphenToPrefix { get; set; }
}
