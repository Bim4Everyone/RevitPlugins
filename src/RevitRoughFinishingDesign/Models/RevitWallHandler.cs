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
        private Line _wallLine;

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

        public Line GetWallLineOnActiveView() {
            if(_wallLine != null) {
                return _wallLine;
            } else {
                XYZ centroidPoint = GetWallCentroidPointOnActiveView();
                double zPoint = centroidPoint.Z;

                LocationCurve wallLine = _wallElement.Location as LocationCurve;
                if(wallLine.Curve is Line) {
                    Curve wallCurve = _revitRepository.TransformCurveToExactZ(wallLine.Curve, zPoint);
                    _wallLine = wallCurve as Line;
                    return _wallLine;
                } else {
                    throw new Exception("Дуги не обрабатываются");
                }
            }
        }

        public Line GetWallLineFromSolid() {
            XYZ centroidSolidWallPoint = GetWallSolidCentroidPoint();
            double zPoint = centroidSolidWallPoint.Z;

            LocationCurve wallLine = _wallElement.Location as LocationCurve;
            Curve wallCurve = _revitRepository.TransformCurveToExactZ(wallLine.Curve, zPoint);
            return wallCurve as Line;
        }

        //private Line GetWallLineInMiddlePoint(Line wallLine) {
        //    XYZ wallCentroidPoint = GetWallCentroidPointOnActiveView();
        //    double wallLength = wallLine.Length;
        //    XYZ alongsideVector = wallLine.Direction.Normalize();
        //    XYZ startPoint = wallCentroidPoint + alongsideVector * (wallLength / 2);
        //    XYZ endPoint = wallCentroidPoint - alongsideVector * (wallLength / 2);
        //    return Line.CreateBound(startPoint, endPoint);
        //}

        public IList<Line> GetWallLinesForDraw() {
            IList<Line> insideLines = new List<Line>();
            SolidCurveIntersectionOptions intersectOptInside = new SolidCurveIntersectionOptions() {
                ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
            };
            Solid wallSolid = GetWallSolid();
            Line wallLine = GetWallLineFromSolid();
            double zPoint = _revitRepository.GetVerticalPointFromActiveView();
            SolidCurveIntersection intersection = wallSolid.IntersectWithCurve(wallLine, intersectOptInside);
            if(intersection.SegmentCount > 0) {
                for(int i = 0; i < intersection.SegmentCount; i++) {
                    Curve segment = intersection.GetCurveSegment(i);
                    if(segment is Line lineSegment) {
                        insideLines.Add(_revitRepository.TransformLineToExactZ(lineSegment, zPoint));
                    }
                }
            }
            return insideLines;
        }

        public XYZ GetWallCentroidPointOnActiveView() {
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

        public XYZ GetWallSolidCentroidPoint() {
            Solid wallSolid = GetWallSolid();
            XYZ wallCentroidPoint = wallSolid.ComputeCentroid();
            return wallCentroidPoint;
        }

        /// <summary>
        /// Возвращает направление вектора внутрь помещения
        /// </summary>
        /// <returns></returns>
        public XYZ GetDirectionToRoom() {
            if(_directionToRoom != null) {
                return _directionToRoom;
            } else {
                Line wallCurve = GetWallLineOnActiveView();
                XYZ wallCentroidPoint = GetWallCentroidPointOnActiveView();
                XYZ normalVector = XYZ.BasisZ.CrossProduct(wallCurve.Direction).Normalize();

                IList<XYZ> rightPoints = new List<XYZ>();
                IList<XYZ> leftPoints = new List<XYZ>();

                for(double step = _step; step < _step * _numberOfPoints; step += _step) {
                    XYZ rightPoint = new XYZ(wallCentroidPoint.X, wallCentroidPoint.Y, wallCentroidPoint.Z)
                        + normalVector * step;
                    XYZ leftPoint = new XYZ(wallCentroidPoint.X, wallCentroidPoint.Y, wallCentroidPoint.Z)
                        - normalVector * step;
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
    }
}
