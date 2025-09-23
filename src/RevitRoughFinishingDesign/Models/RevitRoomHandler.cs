using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit.Geometry;

using RevitRoughFinishingDesign.Services;

namespace RevitRoughFinishingDesign.Models;
internal class RevitRoomHandler {
    private readonly RevitRepository _revitRepository;
    private Solid _roomSolid;
    private Outline _roomOutline;
    private ICollection<ElementId> _wallsInRoom;
    private readonly ICurveLoopsSimplifier _curveLoopsSimplifier;
    private Solid _roomActiveViewSolid;
    private IList<CurveLoop> _simplifiedCurveList;
    private IList<RoomBorder> _roomBorders;

    /// <summary>
    /// Смещение границ BoundingBox помещения, мм
    /// </summary>
    private const double _offsetToOutline = 500;

    private const double _roomActiveViewSolidHeight = 0.032; //~10 мм

    public RevitRoomHandler(RevitRepository revitRepository, Room room,
        ICurveLoopsSimplifier curveLoopsSimplifier) {
        _revitRepository = revitRepository;
        RevitRoom = room;
        _curveLoopsSimplifier = curveLoopsSimplifier;
    }
    public Room RevitRoom { get; }
    private Solid GetRoomSolid() {
        var options = new Options() { DetailLevel = ViewDetailLevel.Fine };
        _roomSolid ??= RevitRoom.GetSolids(options).First();
        return _roomSolid;
    }

    private Outline GetRoomOutlineWithOffset() {
        if(_roomOutline != null) {
            return _roomOutline;
        } else {
            var roomSolid = GetRoomSolid();
            double offsetInFeet = _revitRepository.ConvertToFeetFromMillimeters(_offsetToOutline);
            var roomGeometryBoundingBox = roomSolid.GetBoundingBox();

            var roomBoundingBoxWithOffset = new BoundingBoxXYZ() {
                Min = roomGeometryBoundingBox.Transform.OfPoint(roomGeometryBoundingBox.Min),
                Max = roomGeometryBoundingBox.Transform.OfPoint(roomGeometryBoundingBox.Max)
            };

            _roomOutline = new Outline(roomBoundingBoxWithOffset.Min - new XYZ(1, 1, 1) * offsetInFeet,
                roomBoundingBoxWithOffset.Max + new XYZ(1, 1, 1) * offsetInFeet);
            return _roomOutline;
        }
    }

    private XYZ GetGeometryCenterPointFromWall(Element wall) {
        var options = new Options() { DetailLevel = ViewDetailLevel.Fine };
        var wallSolid = wall.GetSolids(options).First();
        var geometryCenter = wallSolid.ComputeCentroid();
        return geometryCenter;
    }

    /// <summary>
    /// Создает Solid помещения по линии секущего диапазона активного вида, толщиной 
    /// <see cref="_roomActiveViewSolidHeight"/>
    /// </summary>
    /// <returns></returns>
    public Solid GetRoomViewPlanSolid() {
        if(_roomActiveViewSolid != null) {
            return _roomActiveViewSolid;
        } else {

            double zPoint = _revitRepository.GetVerticalPointFromActiveView();
            var roomBoundaries = _revitRepository.GetBoundarySegments(RevitRoom);
            var curveLoops = new List<CurveLoop>();

            foreach(var boundary in roomBoundaries) {
                // Создаем CurveLoop для текущего контура
                var curveLoop = new CurveLoop();

                foreach(var segment in boundary) {
                    var curve = _revitRepository.TransformCurveToExactZ(segment.GetCurve(), zPoint);
                    curveLoop.Append(curve);
                }

                // Добавляем CurveLoop в список
                curveLoops.Add(curveLoop);
            }

            _roomActiveViewSolid = GeometryCreationUtilities.CreateExtrusionGeometry(
                curveLoops, -XYZ.BasisZ, _roomActiveViewSolidHeight);
            return _roomActiveViewSolid;
        }
    }


    public IList<CurveLoop> GetSimplifiedCurveLoops() {
        if(_simplifiedCurveList != null) {
            return _simplifiedCurveList;
        } else {
            var spatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
            var boundarySegments = RevitRoom.GetBoundarySegments(spatialElementBoundaryOptions);
            IList<CurveLoop> simplifiedCurveLoops = [];
            double zPoint = _revitRepository.GetVerticalPointFromActiveView();
            foreach(var boundarySegment in boundarySegments) {

                var curveLoop = new CurveLoop();
                foreach(var curve in boundarySegment) {
                    var oldCurve = curve.GetCurve();
                    var newCorrectCurve = _revitRepository.TransformCurveToExactZ(oldCurve, zPoint);
                    curveLoop.Append(newCorrectCurve);
                }

                var simplifiedLoop = _curveLoopsSimplifier.Simplify(curveLoop);
                simplifiedCurveLoops.Add(simplifiedLoop);
                _simplifiedCurveList = simplifiedCurveLoops;
            }
        }
        return _simplifiedCurveList;
    }

    public IList<RoomBorder> GetRoomBorders() {
        if(_roomBorders != null) {
            return _roomBorders;
        } else {
            var simplifiedLoops = GetSimplifiedCurveLoops();
            IList<RoomBorder> roomBorders = [];

            foreach(var loop in simplifiedLoops) {
                foreach(var curve in loop) {
                    roomBorders.Add(new RoomBorder(curve));
                }
            }
            _roomBorders = roomBorders;
            return _roomBorders;
        }
    }

    public void CreateTestSolids() {
        var roomActiveSolid = GetRoomViewPlanSolid();
        var walls = GetWallsFromRoom();
        foreach(var wallId in walls) {
            var wallSolid = _revitRepository.Document.GetElement(wallId).GetSolids().First();
            var correctWallSolid = BooleanOperationsUtils.ExecuteBooleanOperation(
                wallSolid, roomActiveSolid, BooleanOperationsType.Intersect);
            _revitRepository.CreateDirectShape(correctWallSolid);
        }
    }

    public void CreateTestLines() {
        var roomWalls = GetWallsFromRoom();
        foreach(var wallId in roomWalls) {
            var wall = _revitRepository.Document.GetElement(wallId) as Wall;
            var revitWallHandler = new RevitWallHandler(_revitRepository, RevitRoom, wall);
            var wallLines = revitWallHandler.GetWallLinesForDraw();
            foreach(var line in wallLines) {
                _revitRepository.CreateTestModelLine(line);
            }
        }
    }

    public ICollection<ElementId> GetWallsFromRoom() {
        if(_wallsInRoom != null) {
            return _wallsInRoom;
        } else {
            var roomOutlineWithOffset = GetRoomOutlineWithOffset();

            var boundingBoxIntersectsFilter =
                new BoundingBoxIntersectsFilter(roomOutlineWithOffset);
            _wallsInRoom = new FilteredElementCollector(_revitRepository.Document,
                _revitRepository.GetWallsIds())
                .WherePasses(boundingBoxIntersectsFilter)
                .Where(el => RevitRoom.IsPointInRoom(GetGeometryCenterPointFromWall(el)))
                .Select(el => el.Id)
                .ToList();
            return _wallsInRoom;
        }
    }

}
