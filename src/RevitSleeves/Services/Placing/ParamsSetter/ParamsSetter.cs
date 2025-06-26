using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal abstract class ParamsSetter {
    protected void SetElevation(FamilyInstance sleeve, XYZ point) {
        var level = (Level) sleeve.Document.GetElement(sleeve.LevelId);
        sleeve.SetParamValue(BuiltInParameter.INSTANCE_ELEVATION_PARAM, point.Z - level.Elevation);
    }
}
