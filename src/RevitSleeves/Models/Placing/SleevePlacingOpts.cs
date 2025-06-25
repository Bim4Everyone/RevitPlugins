using Autodesk.Revit.DB;

using RevitSleeves.Services.Placing.ParamsSetter;

namespace RevitSleeves.Models.Placing;
internal class SleevePlacingOpts {
    public SleevePlacingOpts() {

    }


    public XYZ Point { get; set; }

    public Level Level { get; set; }

    public Rotation Rotation { get; set; }

    public FamilySymbol FamilySymbol { get; set; }

    public IParamsSetter ParamsSetter { get; set; }

    public Element[] DependentElements { get; set; }
}
