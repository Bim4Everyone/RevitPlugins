using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit.Geometry;

namespace RevitRoughFinishingDesign.Models {
    internal class RevitWallHandler {
        private readonly RevitRepository _revitRepository;
        private readonly Room _room;
        private readonly Element _wallElement;
        private XYZ _directionToRoom;
        private Solid _wallSolid;
        private XYZ _wallCentroidPoint;
        private ElementId _wallTypeId;

        /// <summary>
        /// Шаг создания точек для проверки направления
        /// </summary>
        private const double _step = 0.032; //~10 мм

        /// <summary>
        /// Количество точек
        /// </summary>
        private const int _numberOfPoints = 10;

        public RevitWallHandler(RevitRepository revitRepository, Room room, Element wall) {
            _revitRepository = revitRepository;
            _room = room;
            _wallElement = wall;
        }

        private Solid GetWallSolid() {
            if(_wallSolid != null) {
                return _wallSolid;
            } else {
                _wallSolid = _wallElement.GetSolids().First();
                return _wallSolid;
            }
        }

        public ElementId GetWallTypeId() {
            if(_wallTypeId != null) {
                return _wallTypeId;
            } else {
                Wall wall = _wallElement as Wall;
                ElementId wallTypeId = wall.WallType.Id;
                _wallTypeId = wallTypeId;
                return _wallTypeId;
            }
        }

        public Line GetWallLine() {
            double zPoint = _revitRepository.GetVerticalPointFromActiveView();
            LocationCurve wallLine = _wallElement.Location as LocationCurve;
            if(wallLine.Curve is Line) {
                Curve wallCurve = _revitRepository.TransformCurveToExactZ(wallLine.Curve, zPoint);

                return wallCurve as Line;
            } else {
                throw new Exception("Дуги не обрабатываются");
            }
        }

        public IList<Line> GetWallLinesForDraw() {
            IList<Line> insideLines = new List<Line>();
            SolidCurveIntersectionOptions intersectOptInside = new SolidCurveIntersectionOptions() {
                ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
            };
            Solid wallSolid = GetWallSolid();
            Line wallLine = GetWallLine();
            SolidCurveIntersection intersection = wallSolid.IntersectWithCurve(wallLine, intersectOptInside);

            if(intersection.SegmentCount > 0) {
                for(int i = 0; i < intersection.SegmentCount; i++) {
                    Curve segment = intersection.GetCurveSegment(i);
                    if(segment is Line lineSegment) {
                        insideLines.Add(lineSegment);
                    }
                }
            }
            return insideLines;
        }

        public XYZ GetWallCentroidPoint() {
            if(_wallCentroidPoint != null) {
                return _wallCentroidPoint;
            } else {

                double zPoint = _revitRepository.GetVerticalPointFromActiveView();
                Solid wallSolid = GetWallSolid();
                XYZ wallCentroidPoint = _revitRepository.TransformXYZToExactZ(wallSolid.ComputeCentroid(), zPoint);
                _wallCentroidPoint = wallCentroidPoint;
                return _wallCentroidPoint;
            }
        }
        /// <summary>
        /// Возвращает направление вектора внутрь помещения
        /// </summary>
        /// <returns></returns>
        public XYZ GetDirectionToRoom() {
            if(_directionToRoom != null) {
                return _directionToRoom;
            } else {
                Line wallCurve = GetWallLine();
                XYZ wallCentroidPoint = GetWallCentroidPoint();
                XYZ normalVector = XYZ.BasisZ.CrossProduct(wallCurve.Direction).Normalize();

                IList<XYZ> rightPoints = new List<XYZ>();
                IList<XYZ> leftPoints = new List<XYZ>();

                for(double step = _step; step < _step * _numberOfPoints; step += _step) {
                    XYZ rightPoint = new XYZ(wallCentroidPoint.X, wallCentroidPoint.Y, wallCentroidPoint.Z)
                        + normalVector * step;
                    XYZ leftPoint = new XYZ(wallCentroidPoint.X, wallCentroidPoint.Y, wallCentroidPoint.Z)
                        - normalVector * step;
                    Line rLine = Line.CreateBound(rightPoint, new XYZ(0, 0, 0));
                    Line lLine = Line.CreateBound(leftPoint, new XYZ(0, 0, 0));
                    //_revitRepository.CreateTestModelLine(rLine);
                    //_revitRepository.CreateTestModelLine(lLine);
                    if(_room.IsPointInRoom(rightPoint)) {
                        rightPoints.Add(rightPoint);
                    }
                    if(_room.IsPointInRoom(leftPoint)) {
                        leftPoints.Add(leftPoint);
                    }
                }

                if(rightPoints.Count >= leftPoints.Count) {
                    _directionToRoom = normalVector;
                } else {
                    _directionToRoom = -normalVector;
                }
                return _directionToRoom;
            }
        }

        public static implicit operator RevitWallHandler(RevitRoomHandler v) {
            throw new NotImplementedException();
        }
    }
}
