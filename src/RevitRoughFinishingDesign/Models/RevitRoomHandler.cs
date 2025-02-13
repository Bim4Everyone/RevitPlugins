using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit.Geometry;

using RevitRoughFinishingDesign.Services;

namespace RevitRoughFinishingDesign.Models {
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
            Options options = new Options() { DetailLevel = ViewDetailLevel.Fine };
            _roomSolid = _roomSolid ?? RevitRoom.GetSolids(options).First();
            return _roomSolid;
        }

        private Outline GetRoomOutlineWithOffset() {
            if(_roomOutline != null) {
                return _roomOutline;
            } else {
                Solid roomSolid = GetRoomSolid();
                double offsetInFeet = _revitRepository.ConvertToFeetFromMillimeters(_offsetToOutline);
                BoundingBoxXYZ roomGeometryBoundingBox = roomSolid.GetBoundingBox();

                BoundingBoxXYZ roomBoundingBoxWithOffset = new BoundingBoxXYZ() {
                    Min = roomGeometryBoundingBox.Transform.OfPoint(roomGeometryBoundingBox.Min),
                    Max = roomGeometryBoundingBox.Transform.OfPoint(roomGeometryBoundingBox.Max)
                };

                _roomOutline = new Outline(roomBoundingBoxWithOffset.Min - new XYZ(1, 1, 1) * offsetInFeet,
                    roomBoundingBoxWithOffset.Max + new XYZ(1, 1, 1) * offsetInFeet);
                return _roomOutline;
            }
        }

        private XYZ GetGeometryCenterPointFromWall(Element wall) {
            Options options = new Options() { DetailLevel = ViewDetailLevel.Fine };
            Solid wallSolid = wall.GetSolids(options).First();
            XYZ geometryCenter = wallSolid.ComputeCentroid();
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
                IList<IList<BoundarySegment>> roomBoundaries = _revitRepository.GetBoundarySegments(RevitRoom);
                List<CurveLoop> curveLoops = new List<CurveLoop>();

                foreach(IList<BoundarySegment> boundary in roomBoundaries) {
                    // Создаем CurveLoop для текущего контура
                    CurveLoop curveLoop = new CurveLoop();

                    foreach(BoundarySegment segment in boundary) {
                        Curve curve = _revitRepository.TransformCurveToExactZ(segment.GetCurve(), zPoint);
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
                SpatialElementBoundaryOptions spatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
                IList<IList<BoundarySegment>> boundarySegments = RevitRoom.GetBoundarySegments(spatialElementBoundaryOptions);
                IList<CurveLoop> simplifiedCurveLoops = new List<CurveLoop>();
                double zPoint = _revitRepository.GetVerticalPointFromActiveView();
                foreach(var boundarySegment in boundarySegments) {

                    CurveLoop curveLoop = new CurveLoop();
                    foreach(var curve in boundarySegment) {
                        Curve oldCurve = curve.GetCurve();
                        Curve newCorrectCurve = _revitRepository.TransformCurveToExactZ(oldCurve, zPoint);
                        curveLoop.Append(newCorrectCurve);
                    }

                    CurveLoop simplifiedLoop = _curveLoopsSimplifier.Simplify(curveLoop);
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
                IList<CurveLoop> simplifiedLoops = GetSimplifiedCurveLoops();
                IList<RoomBorder> roomBorders = new List<RoomBorder>();

                foreach(CurveLoop loop in simplifiedLoops) {
                    foreach(Curve curve in loop) {
                        roomBorders.Add(new RoomBorder(curve));
                    }
                }
                _roomBorders = roomBorders;
                return _roomBorders;
            }
        }

        public void CreateTestSolids() {
            Solid roomActiveSolid = GetRoomViewPlanSolid();
            ICollection<ElementId> walls = GetWallsFromRoom();
            foreach(ElementId wallId in walls) {
                Solid wallSolid = _revitRepository.Document.GetElement(wallId).GetSolids().First();
                Solid correctWallSolid = BooleanOperationsUtils.ExecuteBooleanOperation(
                    wallSolid, roomActiveSolid, BooleanOperationsType.Intersect);
                _revitRepository.CreateDirectShape(correctWallSolid);
            }
        }

        public void CreateTestLines() {
            ICollection<ElementId> roomWalls = GetWallsFromRoom();
            foreach(ElementId wallId in roomWalls) {
                Wall wall = _revitRepository.Document.GetElement(wallId) as Wall;
                RevitWallHandler revitWallHandler = new RevitWallHandler(_revitRepository, RevitRoom, wall);
                IList<Line> wallLines = revitWallHandler.GetWallLinesForDraw();
                foreach(Line line in wallLines) {
                    _revitRepository.CreateTestModelLine(line);
                }
            }
        }

        public ICollection<ElementId> GetWallsFromRoom() {
            if(_wallsInRoom != null) {
                return _wallsInRoom;
            } else {
                Outline roomOutlineWithOffset = GetRoomOutlineWithOffset();

                BoundingBoxIntersectsFilter boundingBoxIntersectsFilter =
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

        //public WallDesignData GetWallDesignData(Element wall) {
        //    double zPoint = _revitRepository.GetVerticalPointFromActiveView();
        //    LocationCurve wallLine = wall.Location as LocationCurve;
        //    Curve wallCurve = TransformCurveToExactZ(wallLine.Curve, zPoint);

        //}
    }
}
