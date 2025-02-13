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
        private readonly Element _wall;
        private XYZ _directionToRoom;
        private Solid _wallSolid;

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
            _wall = wall;
        }

        private Solid GetWallSolid() {
            if(_wallSolid != null) {
                return _wallSolid;
            } else {
                _wallSolid = _wall.GetSolids().First();
                return _wallSolid;
            }
        }

        public Line GetWallLine() {
            double zPoint = _revitRepository.GetVerticalPointFromActiveView();
            LocationCurve wallLine = _wall.Location as LocationCurve;
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

        /// <summary>
        /// Возвращает направление вектора внутрь помещения
        /// </summary>
        /// <returns></returns>
        public XYZ GetDirectionToRoom() {
            if(_directionToRoom != null) {
                return _directionToRoom;
            } else {
                Line wallCurve = GetWallLine();
                XYZ wallCurveCenter = wallCurve.Evaluate(0.5, false);
                XYZ normalVector = XYZ.BasisZ.CrossProduct(wallCurve.Direction).Normalize();
                XYZ secondBecor = XYZ.BasisZ.CrossProduct(wallCurve.Direction.Negate()).Normalize();

                IList<XYZ> rightPoints = new List<XYZ>();
                IList<XYZ> leftPoints = new List<XYZ>();

                for(double step = _step; step < _step * _numberOfPoints; step += _step) {
                    XYZ rightPoint = new XYZ(wallCurveCenter.X, wallCurveCenter.Y, wallCurveCenter.Z)
                        + normalVector * step;
                    XYZ leftPoint = new XYZ(wallCurveCenter.X, wallCurveCenter.Y, wallCurveCenter.Z)
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

        public static implicit operator RevitWallHandler(RevitRoomHandler v) {
            throw new NotImplementedException();
        }
    }
}
