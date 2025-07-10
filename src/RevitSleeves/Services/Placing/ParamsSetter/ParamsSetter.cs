using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitSleeves.Services.Core;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal abstract class ParamsSetter {
    /// <summary>
    /// Назначает отметку от уровня
    /// </summary>
    /// <param name="sleeve">Гильза</param>
    /// <param name="point">Точка вставки гильзы</param>
    protected void SetElevation(FamilyInstance sleeve, XYZ point) {
        var level = (Level) sleeve.Document.GetElement(sleeve.LevelId);
        sleeve.SetParamValue(BuiltInParameter.INSTANCE_ELEVATION_PARAM, point.Z - level.ProjectElevation);
    }

    /// <summary>
    /// Назначает угол наклона
    /// </summary>
    /// <param name="sleeve">Гильза</param>
    /// <param name="inclineAngle">Угол в радианах</param>
    protected void SetInclineAngle(FamilyInstance sleeve, double inclineAngle) {
        sleeve.SetSharedParamValue(NamesProvider.ParameterSleeveIncline, inclineAngle);
    }

    protected void SetLength(FamilyInstance sleeve, double length) {
        sleeve.SetParamValue(NamesProvider.ParameterSleeveLength, length);
    }

    protected void SetDiameter(FamilyInstance sleeve, double diameter) {
        sleeve.SetParamValue(NamesProvider.ParameterSleeveDiameter, diameter);
    }
}
