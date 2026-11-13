using Autodesk.Revit.DB;

namespace RevitAreaBoundaries.Services;

internal interface IBoundingBoxService {
    BoundingBoxXYZ GetBoundingBoxXyz(Element element);
}
