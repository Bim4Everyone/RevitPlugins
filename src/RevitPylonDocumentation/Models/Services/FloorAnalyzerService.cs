using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PylonSheetNView;

namespace RevitPylonDocumentation.Models.Services;
internal class FloorAnalyzerService {
    private readonly Document _doc;
    private readonly PylonSheetInfo _sheetInfo;

    public FloorAnalyzerService(Document document, PylonSheetInfo pylonSheetInfo) {
        _doc = document;
        _sheetInfo = pylonSheetInfo;
    }


    internal PlanarFace GetTopFloorFace(Element floor, Options options) => GetFloorFace(floor, options)
    .FirstOrDefault(face => Math.Abs(face.FaceNormal.Z - 1) < 0.001);

    internal PlanarFace GetBottomFloorFace(Element floor, Options options) => GetFloorFace(floor, options)
        .FirstOrDefault(face => Math.Abs(face.FaceNormal.Z + 1) < 0.001);


    private IEnumerable<PlanarFace> GetFloorFace(Element floor, Options options) => floor.get_Geometry(options)?
        .OfType<Solid>()
        .Where(solid => solid?.Volume > 0)
        .SelectMany(solid => solid.Faces.OfType<PlanarFace>());


    internal Element GetLastFloor() {
        var lastPylon = _sheetInfo.HostElems.Last();
        var bbox = lastPylon.get_BoundingBox(null);

        // Готовим фильтр для сбор плит в области вокруг верхней точки пилона
        // Для поиска берется просто ближайшая область
        var outline = new Outline(
            bbox.Max - new XYZ(10, 10, 5),
            bbox.Max + new XYZ(10, 10, 5)
        );
        var filter = new BoundingBoxIntersectsFilter(outline);

        return new FilteredElementCollector(_doc)
            .OfCategory(BuiltInCategory.OST_Floors)
            .WherePasses(filter)
            .WhereElementIsNotElementType()
            .FirstOrDefault();
    }
}
