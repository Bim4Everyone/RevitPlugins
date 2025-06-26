using Autodesk.Revit.DB;

namespace RevitSleeves.Services.Core;
internal interface IGeometryUtils {
    bool FloorIsHorizontal(Floor floor);

    double GetFloorThickness(Floor structureElement);
}
