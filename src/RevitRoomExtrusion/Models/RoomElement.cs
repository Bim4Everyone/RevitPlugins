using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;


namespace RevitRoomExtrusion.Models;
internal class RoomElement {
    private readonly RevitRepository _revitRepository;
    private readonly Room _room;
    private readonly View3D _view3D;
    private readonly double _normalDirection = -100000;
    private readonly SpatialElementBoundaryOptions _options = new();

    private static readonly ElementMulticategoryFilter _multiCategoryFilter = new(
        [
            BuiltInCategory.OST_StructuralFoundation,
            BuiltInCategory.OST_Floors
        ]);

    public RoomElement(RevitRepository revitRepository, Room room, View3D view3D) {
        _revitRepository = revitRepository;
        _room = room;
        _view3D = view3D;
    }

    // Метод получения точки размещения помещения
    public double GetOriginalLocationPoint() {
        var locationPoint = _room.Location as LocationPoint;
        return locationPoint.Point.Z;
    }

    // Метод получения отметки чистого пола, над которым размещено помещение
    public int GetCleanFloorLocationPoint() {
        var boundingBox = _room.get_BoundingBox(null);
        var pointCenter = (boundingBox.Max + boundingBox.Min) / 2;
        var pointDirection = new XYZ(pointCenter.X, pointCenter.Y, _normalDirection);

        var referenceWithContext = GetReferenceWithContext(pointCenter, pointDirection);
        double foundElementLocation;
        if(referenceWithContext != null) {
            double proximity = referenceWithContext.Proximity;
            foundElementLocation = pointCenter.Z - proximity - _revitRepository.GetBasePointLocation();
        } else {
            foundElementLocation = boundingBox.Min.Z;
        }
        double convertedFoundElementLocation = UnitUtils.ConvertFromInternalUnits(
            foundElementLocation, UnitTypeId.Millimeters);
        return Convert.ToInt32(Math.Round(convertedFoundElementLocation));
    }

    // Метод получения объекта ReferenceWithContext
    private ReferenceWithContext GetReferenceWithContext(XYZ pointCenter, XYZ pointDirection) {
        var refIntersec = new ReferenceIntersector(
            _multiCategoryFilter, FindReferenceTarget.Element, _view3D);
        return refIntersec.FindNearest(pointCenter, pointDirection);
    }

    // Метод получения границ помещения в виде массива кривых
    public CurveArrArray GetBoundaryArrArray() {
        var listListSegments = _room.GetBoundarySegments(_options);
        var curveArrArray = new CurveArrArray();
        foreach(var listSegments in listListSegments) {
            var curveArray = new CurveArray();
            foreach(var boundarySegment in listSegments) {
                var curve = boundarySegment.GetCurve();
                curveArray.Append(curve);
            }
            curveArrArray.Append(curveArray);
        }
        return curveArrArray;
    }
}
