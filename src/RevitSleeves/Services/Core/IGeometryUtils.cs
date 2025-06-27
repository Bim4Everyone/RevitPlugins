using Autodesk.Revit.DB;

namespace RevitSleeves.Services.Core;
internal interface IGeometryUtils {
    bool IsHorizontal(Floor floor);

    double GetFloorThickness(Floor structureElement);

    bool IsVertical(MEPCurve curve);

    bool IsHorizontal(MEPCurve curve);
}
