using Autodesk.Revit.DB;

namespace RevitDeclarations.Models;
internal class ApartmentsSettings : DeclarationSettings {
    public Parameter ApartmentFullNumberParam { get; set; }
    public Parameter BuildingNumberParam { get; set; }
    public Parameter ConstrWorksNumberParam { get; set; }

    public Parameter ApartmentAreaCoefParam { get; set; }
    public Parameter ApartmentAreaLivingParam { get; set; }
    public Parameter ApartmentAreaNonSumParam { get; set; }

    public Parameter RoomsAmountParam { get; set; }
    public Parameter RoomsHeightParam { get; set; }
}
